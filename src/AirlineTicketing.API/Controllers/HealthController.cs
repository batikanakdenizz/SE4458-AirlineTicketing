using AirlineTicketing.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        var canConnect = await _context.Database.CanConnectAsync();
        var pendingMigrations = canConnect
            ? (await _context.Database.GetPendingMigrationsAsync()).ToList()
            : new List<string>();

        if (!canConnect || pendingMigrations.Count > 0)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                database = canConnect ? "Connected" : "Unavailable",
                pendingMigrations,
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            status = "Healthy",
            database = "Connected",
            pendingMigrations,
            timestamp = DateTime.UtcNow
        });
    }
}
