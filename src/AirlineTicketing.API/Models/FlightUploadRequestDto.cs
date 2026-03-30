using Microsoft.AspNetCore.Http;

namespace AirlineTicketing.API.Models;

public class FlightUploadRequestDto
{
    public IFormFile File { get; set; } = null!;
}