namespace AirlineTicketing.Application.DTOs.Flight;

public class FlightDetailsResponseDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int Capacity { get; set; }
    public int AvailableSeats { get; set; }
    public int BookedSeats { get; set; }
    public int CheckedInPassengers { get; set; }
    public int BoardedPassengers { get; set; }
    public string Status { get; set; } = string.Empty;
}
