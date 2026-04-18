namespace AirlineTicketing.Application.DTOs.Booking;

public class BookingResponseDto
{
    public string PnrCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<PassengerResponseDto> Passengers { get; set; } = new();
    public List<TicketSummaryDto> Tickets { get; set; } = new();
}
