namespace AirlineTicketing.Application.DTOs.Booking;

public class TicketSummaryDto
{
    public string TicketNumber { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
