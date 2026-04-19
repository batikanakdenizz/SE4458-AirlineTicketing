namespace AirlineTicketing.Application.DTOs.Booking;

public class BookingSearchRequestDto
{
    public string? ContactEmail { get; set; }
    public string? Status { get; set; }
    public DateTime? DepartureDateFrom { get; set; }
    public DateTime? DepartureDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
}
