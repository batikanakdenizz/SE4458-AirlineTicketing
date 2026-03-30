namespace AirlineTicketing.Application.DTOs.CheckIn;

public class CheckInRequestDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public string PassengerName { get; set; } = string.Empty;
}