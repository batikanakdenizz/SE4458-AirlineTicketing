using AirlineTicketing.Domain.Enums;

namespace AirlineTicketing.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public DateTime ExpiresAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public Flight Flight { get; set; } = null!;
    public Payment? Payment { get; set; }
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
