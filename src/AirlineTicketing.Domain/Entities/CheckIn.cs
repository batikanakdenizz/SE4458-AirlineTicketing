namespace AirlineTicketing.Domain.Entities;

public class CheckIn
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int SeatNumber { get; set; }
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
}