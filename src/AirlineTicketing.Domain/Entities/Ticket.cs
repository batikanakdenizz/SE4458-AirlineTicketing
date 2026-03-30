using AirlineTicketing.Domain.Enums;

namespace AirlineTicketing.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public TicketStatus Status { get; set; } = TicketStatus.Purchased;

    public Flight Flight { get; set; } = null!;
    public CheckIn? CheckIn { get; set; }
}