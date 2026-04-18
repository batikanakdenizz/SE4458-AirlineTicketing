namespace AirlineTicketing.Application.DTOs.Booking;

public class PassengerRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Nationality { get; set; }
}
