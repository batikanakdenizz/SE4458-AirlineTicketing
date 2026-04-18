namespace AirlineTicketing.Application.DTOs.Booking;

public class PassengerResponseDto
{
    public int PassengerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
