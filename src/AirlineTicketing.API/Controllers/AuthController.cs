using AirlineTicketing.Application.DTOs.Auth;
using AirlineTicketing.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (response is null)
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(response);
    }
}