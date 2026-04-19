using AirlineTicketing.Application.DTOs.Booking;
using AirlineTicketing.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineTicketing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequestDto request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        var result = await _bookingService.CreateBookingAsync(request, idempotencyKey);
        return Ok(result);
    }

    [HttpGet("{pnrCode}")]
    [Authorize]
    public async Task<IActionResult> GetBooking([FromRoute] string pnrCode)
    {
        var result = await _bookingService.GetBookingAsync(pnrCode);
        return result is null ? NotFound(new { message = "Booking not found." }) : Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> SearchBookings([FromQuery] BookingSearchRequestDto request)
    {
        var result = await _bookingService.SearchBookingsAsync(request);
        return Ok(result);
    }

    [HttpPatch("{pnrCode}/contact")]
    [Authorize]
    public async Task<IActionResult> UpdateContact(
        [FromRoute] string pnrCode,
        [FromBody] UpdateBookingContactRequestDto request)
    {
        var result = await _bookingService.UpdateContactAsync(pnrCode, request);
        return Ok(result);
    }

    [HttpPost("{pnrCode}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] string pnrCode,
        [FromBody] CancelBookingRequestDto request)
    {
        var result = await _bookingService.CancelBookingAsync(pnrCode, request);
        return Ok(result);
    }
}
