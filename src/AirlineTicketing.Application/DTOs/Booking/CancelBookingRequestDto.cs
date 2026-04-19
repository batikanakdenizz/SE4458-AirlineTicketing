namespace AirlineTicketing.Application.DTOs.Booking;

public class CancelBookingRequestDto
{
    public bool RefundPayment { get; set; } = true;
    public string? Reason { get; set; }
}
