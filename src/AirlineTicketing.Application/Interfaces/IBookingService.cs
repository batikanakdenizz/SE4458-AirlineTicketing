using AirlineTicketing.Application.DTOs.Booking;

namespace AirlineTicketing.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, string? idempotencyKey);
    Task<BookingResponseDto?> GetBookingAsync(string pnrCode);
    Task<BookingListResponseDto> SearchBookingsAsync(BookingSearchRequestDto dto);
    Task<BookingResponseDto> UpdateContactAsync(string pnrCode, UpdateBookingContactRequestDto dto);
    Task<CancelBookingResponseDto> CancelBookingAsync(string pnrCode, CancelBookingRequestDto dto);
}
