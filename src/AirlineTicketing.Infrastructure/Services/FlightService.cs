using AirlineTicketing.Application.DTOs;
using AirlineTicketing.Application.DTOs.Flight;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Domain.Enums;
using AirlineTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Services;

public class FlightService : IFlightService
{
    private const int PageSizeLimit = 10;
    private readonly AppDbContext _context;

    public FlightService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateFlightAsync(CreateFlightDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FlightNumber))
        {
            throw new ArgumentException("Flight number is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.AirportFrom) || string.IsNullOrWhiteSpace(dto.AirportTo))
        {
            throw new ArgumentException("Departure and arrival airports are required.");
        }

        if (dto.Capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.");
        }

        if (dto.ArrivalTime <= dto.DepartureTime)
        {
            throw new ArgumentException("Arrival time must be after departure time.");
        }

        var flightNumber = dto.FlightNumber.Trim();
        var departureTime = EnsureUtc(dto.DepartureTime);
        var arrivalTime = EnsureUtc(dto.ArrivalTime);

        var exists = await _context.Flights.AnyAsync(f =>
            f.FlightNumber == flightNumber &&
            f.DepartureTime == departureTime);

        if (exists)
        {
            throw new InvalidOperationException("A flight with the same flight number and departure time already exists.");
        }

        var flight = new Flight
        {
            FlightNumber = flightNumber,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            AirportFrom = dto.AirportFrom.Trim(),
            AirportTo = dto.AirportTo.Trim(),
            Capacity = dto.Capacity,
            AvailableSeats = dto.Capacity,
            DurationMinutes = (int)(arrivalTime - departureTime).TotalMinutes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        return flight.Id;
    }

    public async Task<QueryFlightsResponseDto> QueryFlightsAsync(QueryFlightsRequestDto dto)
    {
        if (dto.IsRoundTrip && (!dto.ReturnDateFrom.HasValue || !dto.ReturnDateTo.HasValue))
        {
            throw new ArgumentException("ReturnDateFrom and ReturnDateTo are required for round-trip queries.");
        }

        var departureDateFrom = EnsureUtc(dto.DepartureDateFrom);
        var departureDateTo = EnsureUtc(dto.DepartureDateTo);

        var outboundBaseQuery = _context.Flights
            .Where(f =>
                f.DepartureTime >= departureDateFrom &&
                f.DepartureTime <= departureDateTo &&
                f.AirportFrom == dto.AirportFrom &&
                f.AirportTo == dto.AirportTo &&
                f.Status != FlightStatus.Cancelled &&
                f.Status != FlightStatus.Departed &&
                f.Status != FlightStatus.Arrived &&
                f.AvailableSeats >= dto.NumberOfPeople)
            .OrderBy(f => f.DepartureTime);

        var outboundFlights = await ToPagedFlightResultAsync(outboundBaseQuery, dto.Page, dto.Size);

        PagedResultDto<FlightQueryItemDto>? returnFlights = null;

        if (dto.IsRoundTrip)
        {
            var returnDateFrom = EnsureUtc(dto.ReturnDateFrom!.Value);
            var returnDateTo = EnsureUtc(dto.ReturnDateTo!.Value);

            var returnBaseQuery = _context.Flights
                .Where(f =>
                    f.DepartureTime >= returnDateFrom &&
                    f.DepartureTime <= returnDateTo &&
                    f.AirportFrom == dto.AirportTo &&
                    f.AirportTo == dto.AirportFrom &&
                    f.Status != FlightStatus.Cancelled &&
                    f.Status != FlightStatus.Departed &&
                    f.Status != FlightStatus.Arrived &&
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

    public async Task<FlightPassengerListResponseDto> GetPassengerListAsync(
        string flightNumber,
        DateTime departureDate,
        int page = 1,
        int size = 10)
    {
        var (dateStart, dateEnd) = GetUtcDateWindow(departureDate);

        var flight = await _context.Flights
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == flightNumber &&
                f.DepartureTime >= dateStart &&
                f.DepartureTime < dateEnd);

        if (flight is null)
        {
            throw new InvalidOperationException("Flight not found.");
        }

        NormalizePaging(ref page, ref size);

        var baseQuery = _context.CheckIns
            .Include(c => c.Ticket)
            .Where(c => c.FlightId == flight.Id)
            .OrderBy(c => c.SeatNumber);

        var totalCount = await baseQuery.CountAsync();

        var passengers = await baseQuery
            .Skip((page - 1) * size)
            .Take(size)
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
            Passengers = passengers,
            Page = page,
            Size = size,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }

    public async Task<FlightUploadResponseDto> UploadFlightsAsync(Stream fileStream)
    {
        var response = new FlightUploadResponseDto();
        var seenFlightKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var stream = new StreamReader(fileStream);

        await stream.ReadLineAsync();
        var lineNumber = 1;

        while (!stream.EndOfStream)
        {
            lineNumber++;
            var line = await stream.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',').Select(part => part.Trim()).ToArray();

            if (parts.Length != 7)
            {
                AddFailedRow(response, lineNumber, "Expected 7 columns.");
                continue;
            }

            var flightNumber = parts[0];
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                AddFailedRow(response, lineNumber, "Flight number is required.");
                continue;
            }

            if (!DateTime.TryParse(parts[1], out var parsedDepartureTime))
            {
                AddFailedRow(response, lineNumber, "Departure time is invalid.");
                continue;
            }

            if (!DateTime.TryParse(parts[2], out var parsedArrivalTime))
            {
                AddFailedRow(response, lineNumber, "Arrival time is invalid.");
                continue;
            }

            var departureTime = EnsureUtc(parsedDepartureTime);
            var arrivalTime = EnsureUtc(parsedArrivalTime);

            if (arrivalTime <= departureTime)
            {
                AddFailedRow(response, lineNumber, "Arrival time must be after departure time.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(parts[3]) || string.IsNullOrWhiteSpace(parts[4]))
            {
                AddFailedRow(response, lineNumber, "Departure and arrival airports are required.");
                continue;
            }

            if (!int.TryParse(parts[6], out var capacity) || capacity <= 0)
            {
                AddFailedRow(response, lineNumber, "Capacity must be greater than zero.");
                continue;
            }

            int.TryParse(parts[5], out var duration);
            if (duration <= 0)
            {
                duration = (int)(arrivalTime - departureTime).TotalMinutes;
            }

            var flightKey = $"{flightNumber}|{departureTime:O}";
            if (!seenFlightKeys.Add(flightKey))
            {
                AddSkippedFlight(response, flightNumber, departureTime);
                continue;
            }

            var exists = await _context.Flights
                .AnyAsync(f => f.FlightNumber == flightNumber && f.DepartureTime == departureTime);

            if (exists)
            {
                AddSkippedFlight(response, flightNumber, departureTime);
                continue;
            }

            var flight = new Flight
            {
                FlightNumber = flightNumber,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                AirportFrom = parts[3],
                AirportTo = parts[4],
                Capacity = capacity,
                AvailableSeats = capacity,
                DurationMinutes = duration,
                CreatedAt = DateTime.UtcNow
            };

            _context.Flights.Add(flight);
            response.CreatedCount++;
        }

        await _context.SaveChangesAsync();

        return response;
    }

    public async Task<FlightDetailsResponseDto?> GetFlightDetailsAsync(string flightNumber, DateTime departureDate)
    {
        var flight = await FindFlightByDateAsync(flightNumber, departureDate);
        return flight is null ? null : await ToFlightDetailsAsync(flight);
    }

    public async Task<FlightDetailsResponseDto> UpdateFlightStatusAsync(
        string flightNumber,
        DateTime departureDate,
        UpdateFlightStatusRequestDto dto)
    {
        if (!Enum.TryParse<FlightStatus>(dto.Status, true, out var requestedStatus))
        {
            throw new ArgumentException("Invalid flight status.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var flight = await FindFlightByDateAsync(flightNumber, departureDate, includeBookings: true);
        if (flight is null)
        {
            throw new KeyNotFoundException("Flight not found.");
        }

        ValidateFlightStatusTransition(flight.Status, requestedStatus);
        flight.Status = requestedStatus;

        if (requestedStatus == FlightStatus.Cancelled)
        {
            await CancelFlightBookingsAsync(flight);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return await ToFlightDetailsAsync(flight);
    }

    public async Task<FlightDetailsResponseDto> DelayFlightAsync(
        string flightNumber,
        DateTime departureDate,
        DelayFlightRequestDto dto)
    {
        var newDepartureTime = EnsureUtc(dto.NewDepartureTime);
        var newArrivalTime = EnsureUtc(dto.NewArrivalTime);

        if (newArrivalTime <= newDepartureTime)
        {
            throw new ArgumentException("New arrival time must be after new departure time.");
        }

        var flight = await FindFlightByDateAsync(flightNumber, departureDate);
        if (flight is null)
        {
            throw new KeyNotFoundException("Flight not found.");
        }

        if (flight.Status is FlightStatus.Cancelled or FlightStatus.Departed or FlightStatus.Arrived)
        {
            throw new InvalidOperationException("Only active scheduled flights can be delayed.");
        }

        var duplicateExists = await _context.Flights.AnyAsync(f =>
            f.Id != flight.Id &&
            f.FlightNumber == flight.FlightNumber &&
            f.DepartureTime == newDepartureTime);

        if (duplicateExists)
        {
            throw new InvalidOperationException("A flight with the same flight number and new departure time already exists.");
        }

        flight.DepartureTime = newDepartureTime;
        flight.ArrivalTime = newArrivalTime;
        flight.DurationMinutes = (int)(newArrivalTime - newDepartureTime).TotalMinutes;
        flight.Status = FlightStatus.Delayed;

        await _context.SaveChangesAsync();

        return await ToFlightDetailsAsync(flight);
    }

    private static async Task<PagedResultDto<FlightQueryItemDto>> ToPagedFlightResultAsync(
        IQueryable<Flight> query,
        int page,
        int size)
    {
        NormalizePaging(ref page, ref size);

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

    private static void NormalizePaging(ref int page, ref int size)
    {
        if (page <= 0) page = 1;
        if (size <= 0) size = PageSizeLimit;
        if (size > PageSizeLimit) size = PageSizeLimit;
    }

    private static void AddSkippedFlight(FlightUploadResponseDto response, string flightNumber, DateTime departureTime)
    {
        response.SkippedCount++;
        response.SkippedFlightNumbers.Add($"{flightNumber} ({departureTime:O})");
    }

    private static void AddFailedRow(FlightUploadResponseDto response, int lineNumber, string reason)
    {
        response.FailedCount++;
        response.FailedRows.Add($"Line {lineNumber}: {reason}");
    }

    private async Task<Flight?> FindFlightByDateAsync(
        string flightNumber,
        DateTime departureDate,
        bool includeBookings = false)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            throw new ArgumentException("Flight number is required.");
        }

        var (dateStart, dateEnd) = GetUtcDateWindow(departureDate);

        var query = _context.Flights.AsQueryable();

        if (includeBookings)
        {
            query = query
                .Include(f => f.Bookings)
                .ThenInclude(b => b.Payment)
                .Include(f => f.Bookings)
                .ThenInclude(b => b.Tickets);
        }

        return await query
            .OrderBy(f => f.DepartureTime)
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == flightNumber.Trim() &&
                f.DepartureTime >= dateStart &&
                f.DepartureTime < dateEnd);
    }

    private async Task<FlightDetailsResponseDto> ToFlightDetailsAsync(Flight flight)
    {
        var ticketStats = await _context.Tickets
            .Where(t => t.FlightId == flight.Id)
            .GroupBy(t => 1)
            .Select(g => new
            {
                BookedSeats = g.Count(t => t.Status != TicketStatus.Cancelled && t.Status != TicketStatus.Refunded),
                BoardedPassengers = g.Count(t => t.Status == TicketStatus.Boarded)
            })
            .FirstOrDefaultAsync();

        var checkedInPassengers = await _context.CheckIns.CountAsync(c => c.FlightId == flight.Id);

        return new FlightDetailsResponseDto
        {
            FlightNumber = flight.FlightNumber,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            AirportFrom = flight.AirportFrom,
            AirportTo = flight.AirportTo,
            DurationMinutes = flight.DurationMinutes,
            Capacity = flight.Capacity,
            AvailableSeats = flight.AvailableSeats,
            BookedSeats = ticketStats?.BookedSeats ?? 0,
            CheckedInPassengers = checkedInPassengers,
            BoardedPassengers = ticketStats?.BoardedPassengers ?? 0,
            Status = flight.Status.ToString()
        };
    }

    private static void ValidateFlightStatusTransition(FlightStatus currentStatus, FlightStatus requestedStatus)
    {
        if (currentStatus == requestedStatus)
        {
            return;
        }

        if (currentStatus == FlightStatus.Arrived)
        {
            throw new InvalidOperationException("Arrived flights cannot move to another status.");
        }

        if (currentStatus == FlightStatus.Cancelled && requestedStatus != FlightStatus.Scheduled)
        {
            throw new InvalidOperationException("Cancelled flights can only be reopened as Scheduled.");
        }

        if (currentStatus == FlightStatus.Departed &&
            requestedStatus is FlightStatus.Scheduled or FlightStatus.Delayed or FlightStatus.Boarding)
        {
            throw new InvalidOperationException("Departed flights cannot move back to pre-departure statuses.");
        }
    }

    private async Task CancelFlightBookingsAsync(Flight flight)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT "Id" FROM "Flights" WHERE "Id" = {flight.Id} FOR UPDATE""");

        foreach (var booking in flight.Bookings)
        {
            if (booking.Status is BookingStatus.Cancelled or BookingStatus.Refunded)
            {
                continue;
            }

            booking.Status = BookingStatus.Refunded;

            if (booking.Payment is not null)
            {
                booking.Payment.Status = PaymentStatus.Refunded;
            }

            foreach (var ticket in booking.Tickets)
            {
                if (ticket.Status is not TicketStatus.Cancelled and not TicketStatus.Refunded and not TicketStatus.Flown)
                {
                    ticket.Status = TicketStatus.Refunded;
                }
            }
        }

        var legacyTickets = await _context.Tickets
            .Where(t => t.FlightId == flight.Id && t.BookingId == null)
            .ToListAsync();

        foreach (var ticket in legacyTickets)
        {
            if (ticket.Status is not TicketStatus.Cancelled and not TicketStatus.Refunded and not TicketStatus.Flown)
            {
                ticket.Status = TicketStatus.Refunded;
            }
        }

        flight.AvailableSeats = flight.Capacity;
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static (DateTime Start, DateTime End) GetUtcDateWindow(DateTime value)
    {
        var utc = EnsureUtc(value);
        var start = DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);
        return (start, start.AddDays(1));
    }
}
