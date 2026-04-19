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

    public async Task<TicketDetailsResponseDto?> GetTicketAsync(string ticketNumber)
    {
        var ticket = await LoadTicketAsync(ticketNumber);
        return ticket is null ? null : ToResponse(ticket);
    }

    public async Task<TicketDetailsResponseDto> CancelTicketAsync(string ticketNumber)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var ticket = await LoadTicketAsync(ticketNumber);
        if (ticket is null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        if (ticket.Status is TicketStatus.Cancelled or TicketStatus.Refunded)
        {
            return ToResponse(ticket);
        }

        if (ticket.Status is TicketStatus.CheckedIn or TicketStatus.Boarded or TicketStatus.Flown)
        {
            throw new InvalidOperationException("Checked-in, boarded, or flown tickets cannot be cancelled.");
        }

        if (ticket.Flight.Status is FlightStatus.Departed or FlightStatus.Arrived)
        {
            throw new InvalidOperationException("Tickets cannot be cancelled after departure.");
        }

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT "Id" FROM "Flights" WHERE "Id" = {ticket.FlightId} FOR UPDATE""");

        ticket.Status = ticket.BookingId.HasValue ? TicketStatus.Refunded : TicketStatus.Cancelled;
        ticket.Flight.AvailableSeats = Math.Min(ticket.Flight.Capacity, ticket.Flight.AvailableSeats + 1);

        if (ticket.Booking is not null && ticket.Booking.Tickets.All(t =>
                t.Id == ticket.Id ||
                t.Status is TicketStatus.Cancelled or TicketStatus.Refunded))
        {
            ticket.Booking.Status = ticket.Booking.Payment?.Status == PaymentStatus.Captured
                ? BookingStatus.Refunded
                : BookingStatus.Cancelled;

            if (ticket.Booking.Payment is not null)
            {
                ticket.Booking.Payment.Status = PaymentStatus.Refunded;
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponse(ticket);
    }

    public async Task<TicketDetailsResponseDto> BoardTicketAsync(string ticketNumber)
    {
        var ticket = await LoadTicketAsync(ticketNumber);
        if (ticket is null)
        {
            throw new KeyNotFoundException("Ticket not found.");
        }

        if (ticket.Flight.Status != FlightStatus.Boarding)
        {
            throw new InvalidOperationException("Flight must be in Boarding status before passengers can board.");
        }

        if (ticket.CheckIn is null)
        {
            throw new InvalidOperationException("Passenger must check in before boarding.");
        }

        if (ticket.Status == TicketStatus.Boarded)
        {
            return ToResponse(ticket);
        }

        if (ticket.Status != TicketStatus.CheckedIn)
        {
            throw new InvalidOperationException("Only checked-in tickets can be boarded.");
        }

        ticket.Status = TicketStatus.Boarded;
        await _context.SaveChangesAsync();

        return ToResponse(ticket);
    }

    private static string GenerateTicketNumber()
    {
        return $"TKT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";
    }

    private async Task<Ticket?> LoadTicketAsync(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            throw new ArgumentException("Ticket number is required.");
        }

        var normalizedTicketNumber = ticketNumber.Trim().ToUpperInvariant();

        return await _context.Tickets
            .Include(t => t.Flight)
            .Include(t => t.CheckIn)
            .Include(t => t.Booking)
            .ThenInclude(b => b!.Payment)
            .Include(t => t.Booking)
            .ThenInclude(b => b!.Tickets)
            .FirstOrDefaultAsync(t => t.TicketNumber == normalizedTicketNumber);
    }

    private static TicketDetailsResponseDto ToResponse(Ticket ticket)
    {
        return new TicketDetailsResponseDto
        {
            TicketNumber = ticket.TicketNumber,
            PassengerName = ticket.PassengerName,
            Status = ticket.Status.ToString(),
            FlightNumber = ticket.Flight.FlightNumber,
            DepartureTime = ticket.Flight.DepartureTime,
            AirportFrom = ticket.Flight.AirportFrom,
            AirportTo = ticket.Flight.AirportTo,
            PnrCode = ticket.Booking?.PnrCode,
            SeatNumber = ticket.CheckIn?.SeatNumber,
            CheckInTime = ticket.CheckIn?.CheckInTime
        };
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
