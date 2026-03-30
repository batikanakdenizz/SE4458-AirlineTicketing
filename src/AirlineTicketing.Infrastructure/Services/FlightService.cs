using AirlineTicketing.Application.DTOs;
using AirlineTicketing.Application.DTOs.Flight;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Services;

public class FlightService : IFlightService
{
    private readonly AppDbContext _context;

    public FlightService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateFlightAsync(CreateFlightDto dto)
    {
        var duration = (int)(dto.ArrivalTime - dto.DepartureTime).TotalMinutes;

        var flight = new Flight
        {
            FlightNumber = dto.FlightNumber,
            DepartureTime = dto.DepartureTime,
            ArrivalTime = dto.ArrivalTime,
            AirportFrom = dto.AirportFrom,
            AirportTo = dto.AirportTo,
            Capacity = dto.Capacity,
            AvailableSeats = dto.Capacity,
            DurationMinutes = duration,
            CreatedAt = DateTime.UtcNow
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        return flight.Id;
    }

    public async Task<QueryFlightsResponseDto> QueryFlightsAsync(QueryFlightsRequestDto dto)
    {
        var outboundBaseQuery = _context.Flights
            .Where(f =>
                f.DepartureTime >= dto.DepartureDateFrom &&
                f.DepartureTime <= dto.DepartureDateTo &&
                f.AirportFrom == dto.AirportFrom &&
                f.AirportTo == dto.AirportTo &&
                f.AvailableSeats >= dto.NumberOfPeople)
            .OrderBy(f => f.DepartureTime);

        var outboundFlights = await ToPagedFlightResultAsync(outboundBaseQuery, dto.Page, dto.Size);

        PagedResultDto<FlightQueryItemDto>? returnFlights = null;

        if (dto.IsRoundTrip)
        {
            if (!dto.ReturnDateFrom.HasValue || !dto.ReturnDateTo.HasValue)
            {
                throw new ArgumentException("ReturnDateFrom and ReturnDateTo are required for round-trip queries.");
            }

            var returnBaseQuery = _context.Flights
                .Where(f =>
                    f.DepartureTime >= dto.ReturnDateFrom.Value &&
                    f.DepartureTime <= dto.ReturnDateTo.Value &&
                    f.AirportFrom == dto.AirportTo &&
                    f.AirportTo == dto.AirportFrom &&
                    f.AvailableSeats >= dto.NumberOfPeople)
                .OrderBy(f => f.DepartureTime);

            returnFlights = await ToPagedFlightResultAsync(returnBaseQuery, dto.Page, dto.Size);
        }

        return new QueryFlightsResponseDto
        {
            OutboundFlights = outboundFlights,
            ReturnFlights = returnFlights
        };
    }

    public async Task<FlightPassengerListResponseDto> GetPassengerListAsync(string flightNumber, DateTime departureDate)
    {
        var flight = await _context.Flights
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == flightNumber &&
                f.DepartureTime.Date == departureDate.Date);

        if (flight is null)
        {
            throw new Exception("Flight not found.");
        }

        var passengers = await _context.CheckIns
            .Include(c => c.Ticket)
            .Where(c => c.Ticket.FlightId == flight.Id)
            .OrderBy(c => c.SeatNumber)
            .Select(c => new PassengerListItemDto
            {
                PassengerName = c.Ticket.PassengerName,
                SeatNumber = c.SeatNumber
            })
            .ToListAsync();

        return new FlightPassengerListResponseDto
        {
            FlightNumber = flight.FlightNumber,
            DepartureTime = flight.DepartureTime,
            Passengers = passengers
        };
    }

    public async Task<FlightUploadResponseDto> UploadFlightsAsync(Stream fileStream)
    {
        var response = new FlightUploadResponseDto();

        using var stream = new StreamReader(fileStream);

        // Header satırını geç
        await stream.ReadLineAsync();

        while (!stream.EndOfStream)
        {
            var line = await stream.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');

            if (parts.Length < 6)
                continue;

            var flightNumber = parts[0];

            var exists = await _context.Flights
                .AnyAsync(f => f.FlightNumber == flightNumber);

            if (exists)
            {
                response.SkippedCount++;
                response.SkippedFlightNumbers.Add(flightNumber);
                continue;
            }

            var departureTime = DateTime.SpecifyKind(
                DateTime.Parse(parts[1]),
                DateTimeKind.Utc);

            var arrivalTime = DateTime.SpecifyKind(
                DateTime.Parse(parts[2]),
                DateTimeKind.Utc);

            var capacity = int.Parse(parts[5]);

            var flight = new Flight
            {
                FlightNumber = flightNumber,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                AirportFrom = parts[3],
                AirportTo = parts[4],
                Capacity = capacity,
                AvailableSeats = capacity,
                DurationMinutes = (int)(arrivalTime - departureTime).TotalMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Flights.Add(flight);
            response.CreatedCount++;
        }

        await _context.SaveChangesAsync();

        return response;
    }

    private static async Task<PagedResultDto<FlightQueryItemDto>> ToPagedFlightResultAsync(
        IQueryable<Flight> query,
        int page,
        int size)
    {
        if (page <= 0) page = 1;
        if (size <= 0) size = 10;

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(f => new FlightQueryItemDto
            {
                FlightNumber = f.FlightNumber,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                AirportFrom = f.AirportFrom,
                AirportTo = f.AirportTo,
                DurationMinutes = f.DurationMinutes,
                AvailableSeats = f.AvailableSeats
            })
            .ToListAsync();

        return new PagedResultDto<FlightQueryItemDto>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }
}