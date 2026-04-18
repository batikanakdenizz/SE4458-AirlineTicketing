namespace AirlineTicketing.Application.DTOs.Flight;

public class FlightUploadResponseDto
{
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> SkippedFlightNumbers { get; set; } = new();
    public List<string> FailedRows { get; set; } = new();
}
