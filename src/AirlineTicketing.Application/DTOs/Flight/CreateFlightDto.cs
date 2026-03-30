namespace AirlineTicketing.Application.DTOs.Flight;

public class CreateFlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;
    public int Capacity { get; set; }
}