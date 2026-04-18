namespace AirlineTicketing.Domain.Entities;

using AirlineTicketing.Domain.Enums;

public class Flight
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int Capacity { get; set; }
    public int AvailableSeats { get; set; }
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
