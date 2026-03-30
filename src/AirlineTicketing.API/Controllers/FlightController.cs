using AirlineTicketing.Application.DTOs.Flight;
using AirlineTicketing.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirlineTicketing.API.Models;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FlightController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightDto dto)
    {
        var id = await _flightService.CreateFlightAsync(dto);

        return Ok(new { FlightId = id });
    }

    [HttpGet("query")]
    [AllowAnonymous]
    public async Task<IActionResult> QueryFlights([FromQuery] QueryFlightsRequestDto dto)
    {
        var result = await _flightService.QueryFlightsAsync(dto);
        return Ok(result);
    }


    [HttpGet("passengers")]
public async Task<IActionResult> GetPassengerList(
    [FromQuery] string flightNumber,
    [FromQuery] DateTime departureDate)
{
    var result = await _flightService.GetPassengerListAsync(flightNumber, departureDate);
    return Ok(result);
}


[HttpPost("upload")]
[Authorize]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadFlights([FromForm] FlightUploadRequestDto request)
{
    if (request.File == null || request.File.Length == 0)
        return BadRequest("File is required.");

    var result = await _flightService.UploadFlightsAsync(request.File.OpenReadStream());

    return Ok(result);
}

}