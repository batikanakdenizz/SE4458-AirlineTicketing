namespace AirlineTicketing.Application.DTOs.Ticket;

public class TicketDetailsResponseDto
{
    public string TicketNumber { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;
    public string? PnrCode { get; set; }
    public int? SeatNumber { get; set; }
    public DateTime? CheckInTime { get; set; }
}
