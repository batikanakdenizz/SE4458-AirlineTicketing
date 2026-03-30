using AirlineTicketing.Application.DTOs.CheckIn;
using AirlineTicketing.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpPost]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto)
    {
        var result = await _checkInService.CheckInAsync(dto);

        if (result.Status != "Success")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}