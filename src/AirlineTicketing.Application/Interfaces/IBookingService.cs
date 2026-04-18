using AirlineTicketing.Application.DTOs.Booking;

namespace AirlineTicketing.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, string? idempotencyKey);
    Task<BookingResponseDto?> GetBookingAsync(string pnrCode);
}
