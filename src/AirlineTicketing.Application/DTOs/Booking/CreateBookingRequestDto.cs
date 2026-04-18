namespace AirlineTicketing.Application.DTOs.Booking;

public class CreateBookingRequestDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public List<PassengerRequestDto> Passengers { get; set; } = new();
}
