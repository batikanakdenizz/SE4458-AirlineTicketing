namespace AirlineTicketing.Application.DTOs.Ticket;

public class BuyTicketRequestDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public List<string> PassengerNames { get; set; } = new();
}