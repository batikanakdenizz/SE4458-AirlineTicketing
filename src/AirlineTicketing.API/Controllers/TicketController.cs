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

    [HttpGet("{ticketNumber}")]
    [Authorize]
    public async Task<IActionResult> GetTicket([FromRoute] string ticketNumber)
    {
        var result = await _ticketService.GetTicketAsync(ticketNumber);
        return result is null ? NotFound(new { message = "Ticket not found." }) : Ok(result);
    }

    [HttpPost("{ticketNumber}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelTicket([FromRoute] string ticketNumber)
    {
        var result = await _ticketService.CancelTicketAsync(ticketNumber);
        return Ok(result);
    }

    [HttpPost("{ticketNumber}/board")]
    [Authorize]
    public async Task<IActionResult> BoardTicket([FromRoute] string ticketNumber)
    {
        var result = await _ticketService.BoardTicketAsync(ticketNumber);
        return Ok(result);
    }
}
