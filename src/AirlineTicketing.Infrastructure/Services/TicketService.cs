using AirlineTicketing.Application.DTOs.Ticket;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Domain.Enums;
using AirlineTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BuyTicketResponseDto> BuyTicketAsync(BuyTicketRequestDto dto)
    {
        if (dto.PassengerNames == null || dto.PassengerNames.Count == 0)
        {
            return new BuyTicketResponseDto
            {
                Status = "No passenger names provided."
            };
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var (dateStart, dateEnd) = GetUtcDateWindow(dto.DepartureDate);

        var flight = await _context.Flights
            .OrderBy(f => f.DepartureTime)
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == dto.FlightNumber &&
                f.DepartureTime >= dateStart &&
                f.DepartureTime < dateEnd &&
                f.Status != FlightStatus.Cancelled &&
                f.Status != FlightStatus.Departed &&
                f.Status != FlightStatus.Arrived);

        if (flight is null)
        {
            return new BuyTicketResponseDto
            {
                Status = "Flight not found."
            };
        }

        var updatedRows = await _context.Flights
            .Where(f => f.Id == flight.Id && f.AvailableSeats >= dto.PassengerNames.Count)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.AvailableSeats, f => f.AvailableSeats - dto.PassengerNames.Count));

        if (updatedRows == 0)
        {
            return new BuyTicketResponseDto
            {
                Status = "Sold out or insufficient seats."
            };
        }

        var ticketNumbers = new List<string>();

        foreach (var passengerName in dto.PassengerNames)
        {
            var ticketNumber = GenerateTicketNumber();

            var ticket = new Ticket
            {
                TicketNumber = ticketNumber,
                FlightId = flight.Id,
                PassengerName = passengerName,
                PurchaseDate = DateTime.UtcNow,
                Status = TicketStatus.Purchased
            };

            _context.Tickets.Add(ticket);
            ticketNumbers.Add(ticketNumber);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new BuyTicketResponseDto
        {
            Status = "Success",
            TicketNumbers = ticketNumbers
        };
    }

    private static string GenerateTicketNumber()
    {
        return $"TKT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
    }

    private static (DateTime Start, DateTime End) GetUtcDateWindow(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        var start = DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);
        return (start, start.AddDays(1));
    }
}
