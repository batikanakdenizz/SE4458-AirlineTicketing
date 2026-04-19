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
        try
        {
            var id = await _flightService.CreateFlightAsync(dto);

            return Ok(new { FlightId = id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("query")]
    [AllowAnonymous]
    public async Task<IActionResult> QueryFlights([FromQuery] QueryFlightsRequestDto dto)
    {
        try
        {
            var result = await _flightService.QueryFlightsAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("passengers")]
    [Authorize]
    public async Task<IActionResult> GetPassengerList(
        [FromQuery] string flightNumber,
        [FromQuery] DateTime departureDate,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        try
        {
            var result = await _flightService.GetPassengerListAsync(flightNumber, departureDate, page, size);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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

    [HttpGet("{flightNumber}")]
    [Authorize]
    public async Task<IActionResult> GetFlightDetails(
        [FromRoute] string flightNumber,
        [FromQuery] DateTime departureDate)
    {
        var result = await _flightService.GetFlightDetailsAsync(flightNumber, departureDate);
        return result is null ? NotFound(new { message = "Flight not found." }) : Ok(result);
    }

    [HttpPatch("{flightNumber}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateFlightStatus(
        [FromRoute] string flightNumber,
        [FromQuery] DateTime departureDate,
        [FromBody] UpdateFlightStatusRequestDto request)
    {
        var result = await _flightService.UpdateFlightStatusAsync(flightNumber, departureDate, request);
        return Ok(result);
    }

    [HttpPatch("{flightNumber}/delay")]
    [Authorize]
    public async Task<IActionResult> DelayFlight(
        [FromRoute] string flightNumber,
        [FromQuery] DateTime departureDate,
        [FromBody] DelayFlightRequestDto request)
    {
        var result = await _flightService.DelayFlightAsync(flightNumber, departureDate, request);
        return Ok(result);
    }

}
