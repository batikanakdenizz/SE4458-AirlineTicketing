namespace AirlineTicketing.Application.DTOs.Flight;

public class FlightQueryItemDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int AvailableSeats { get; set; }
}