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

        var flight = await _context.Flights
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == dto.FlightNumber &&
                f.DepartureTime.Date == dto.DepartureDate.Date);

        if (flight is null)
        {
            return new BuyTicketResponseDto
            {
                Status = "Flight not found."
            };
        }

        if (flight.AvailableSeats < dto.PassengerNames.Count)
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

        flight.AvailableSeats -= dto.PassengerNames.Count;

        await _context.SaveChangesAsync();

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
}