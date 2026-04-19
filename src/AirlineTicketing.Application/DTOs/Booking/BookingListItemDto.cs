namespace AirlineTicketing.Application.DTOs.Booking;

public class BookingListItemDto
{
    public string PnrCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
