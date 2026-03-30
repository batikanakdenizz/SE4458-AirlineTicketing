namespace AirlineTicketing.Application.DTOs.Flight;

public class QueryFlightsRequestDto
{
    public string AirportFrom { get; set; } = string.Empty;
    public string AirportTo { get; set; } = string.Empty;

    public DateTime DepartureDateFrom { get; set; }
    public DateTime DepartureDateTo { get; set; }

    public int NumberOfPeople { get; set; }

    public bool IsRoundTrip { get; set; }

    public DateTime? ReturnDateFrom { get; set; }
    public DateTime? ReturnDateTo { get; set; }

    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
}