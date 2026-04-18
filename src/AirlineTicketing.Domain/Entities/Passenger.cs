namespace AirlineTicketing.Domain.Entities;

public class Passenger
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Nationality { get; set; }

    public Booking Booking { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
