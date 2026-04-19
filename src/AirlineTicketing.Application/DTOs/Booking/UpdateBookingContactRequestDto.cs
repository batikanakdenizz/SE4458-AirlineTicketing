namespace AirlineTicketing.Application.DTOs.Booking;

public class UpdateBookingContactRequestDto
{
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
}
