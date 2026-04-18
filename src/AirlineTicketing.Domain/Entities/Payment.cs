using AirlineTicketing.Domain.Enums;

namespace AirlineTicketing.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string Provider { get; set; } = "DemoPayment";
    public string ProviderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CapturedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}
