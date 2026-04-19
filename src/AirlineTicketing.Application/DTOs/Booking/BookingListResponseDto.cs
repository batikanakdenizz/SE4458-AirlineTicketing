namespace AirlineTicketing.Application.DTOs.Booking;

public class BookingListResponseDto
{
    public List<BookingListItemDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
