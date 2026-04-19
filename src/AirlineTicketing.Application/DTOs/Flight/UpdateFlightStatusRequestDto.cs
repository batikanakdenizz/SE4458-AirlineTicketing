namespace AirlineTicketing.Application.DTOs.Flight;

public class UpdateFlightStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
