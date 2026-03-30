namespace AirlineTicketing.Application.DTOs.Flight;

public class FlightPassengerListResponseDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public List<PassengerListItemDto> Passengers { get; set; } = new();
}