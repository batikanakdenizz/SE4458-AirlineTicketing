namespace AirlineTicketing.Application.DTOs.Booking;

public class CancelBookingResponseDto
{
    public string PnrCode { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public int ReleasedSeats { get; set; }
    public string Message { get; set; } = string.Empty;
}
