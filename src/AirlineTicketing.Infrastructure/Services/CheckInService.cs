using AirlineTicketing.Application.DTOs.CheckIn;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Services;

public class CheckInService : ICheckInService
{
    private readonly AppDbContext _context;

    public CheckInService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Flight)
            .Include(t => t.CheckIn)
            .FirstOrDefaultAsync(t =>
                t.Flight.FlightNumber == dto.FlightNumber &&
                t.Flight.DepartureTime.Date == dto.DepartureDate.Date &&
                t.PassengerName == dto.PassengerName);

        if (ticket is null)
        {
            return new CheckInResponseDto
            {
                Status = "Ticket not found for the given flight and passenger."
            };
        }

        if (ticket.CheckIn is not null)
        {
            return new CheckInResponseDto
            {
                Status = "Passenger already checked in.",
                SeatNumber = ticket.CheckIn.SeatNumber
            };
        }

        var maxSeatNumber = await _context.CheckIns
            .Where(c => c.Ticket.FlightId == ticket.FlightId)
            .Select(c => (int?)c.SeatNumber)
            .MaxAsync() ?? 0;

        var nextSeatNumber = maxSeatNumber + 1;

        var checkIn = new CheckIn
        {
            TicketId = ticket.Id,
            SeatNumber = nextSeatNumber,
            CheckInTime = DateTime.UtcNow
        };

        _context.CheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        return new CheckInResponseDto
        {
            Status = "Success",
            SeatNumber = nextSeatNumber
        };
    }
}