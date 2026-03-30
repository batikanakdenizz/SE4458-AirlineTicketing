namespace AirlineTicketing.Application.DTOs.Ticket;

public class BuyTicketResponseDto
{
    public string Status { get; set; } = string.Empty;
    public List<string> TicketNumbers { get; set; } = new();
}