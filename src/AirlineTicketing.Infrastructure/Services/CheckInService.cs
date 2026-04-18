using AirlineTicketing.Application.DTOs.CheckIn;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Domain.Enums;
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
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var (dateStart, dateEnd) = GetUtcDateWindow(dto.DepartureDate);

        var ticket = await _context.Tickets
            .Include(t => t.Flight)
            .Include(t => t.CheckIn)
            .OrderBy(t => t.Flight.DepartureTime)
            .FirstOrDefaultAsync(t =>
                t.Flight.FlightNumber == dto.FlightNumber &&
                t.Flight.DepartureTime >= dateStart &&
                t.Flight.DepartureTime < dateEnd &&
                t.Flight.Status != FlightStatus.Cancelled &&
                t.Flight.Status != FlightStatus.Departed &&
                t.Flight.Status != FlightStatus.Arrived &&
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

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT "Id" FROM "Flights" WHERE "Id" = {ticket.FlightId} FOR UPDATE""");

        var maxSeatNumber = await _context.CheckIns
            .Where(c => c.FlightId == ticket.FlightId)
            .Select(c => (int?)c.SeatNumber)
            .MaxAsync() ?? 0;

        var nextSeatNumber = maxSeatNumber + 1;

        var checkIn = new CheckIn
        {
            TicketId = ticket.Id,
            FlightId = ticket.FlightId,
            SeatNumber = nextSeatNumber,
            CheckInTime = DateTime.UtcNow
        };

        _context.CheckIns.Add(checkIn);
        ticket.Status = TicketStatus.CheckedIn;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CheckInResponseDto
        {
            Status = "Success",
            SeatNumber = nextSeatNumber
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
