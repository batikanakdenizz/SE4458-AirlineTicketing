using AirlineTicketing.Application.DTOs.Flight;

public class FlightPassengerListResponseDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }

    public List<PassengerListItemDto> Passengers { get; set; } = new();

    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}