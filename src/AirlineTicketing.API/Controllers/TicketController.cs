using AirlineTicketing.Application.DTOs.Ticket;
using AirlineTicketing.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequestDto dto)
    {
        var result = await _ticketService.BuyTicketAsync(dto);

        if (result.Status != "Success")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}