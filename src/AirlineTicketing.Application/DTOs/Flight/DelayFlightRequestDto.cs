namespace AirlineTicketing.Application.DTOs.Flight;

public class DelayFlightRequestDto
{
    public DateTime NewDepartureTime { get; set; }
    public DateTime NewArrivalTime { get; set; }
    public string? Reason { get; set; }
}
