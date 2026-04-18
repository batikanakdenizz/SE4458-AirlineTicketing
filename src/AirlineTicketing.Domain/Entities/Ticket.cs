using AirlineTicketing.Domain.Enums;

namespace AirlineTicketing.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public int? BookingId { get; set; }
    public int? PassengerId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public TicketStatus Status { get; set; } = TicketStatus.Purchased;

    public Flight Flight { get; set; } = null!;
    public Booking? Booking { get; set; }
    public Passenger? Passenger { get; set; }
    public CheckIn? CheckIn { get; set; }
}
