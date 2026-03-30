using AirlineTicketing.Application.DTOs;

namespace AirlineTicketing.Application.DTOs.Flight;

public class QueryFlightsResponseDto
{
    public PagedResultDto<FlightQueryItemDto> OutboundFlights { get; set; } = new();
    public PagedResultDto<FlightQueryItemDto>? ReturnFlights { get; set; }
}