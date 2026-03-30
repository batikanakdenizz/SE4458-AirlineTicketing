namespace AirlineTicketing.Application.DTOs.CheckIn;

public class CheckInResponseDto
{
    public string Status { get; set; } = string.Empty;
    public int? SeatNumber { get; set; }
}