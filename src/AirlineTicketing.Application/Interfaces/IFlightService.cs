using AirlineTicketing.Application.DTOs.Flight;

namespace AirlineTicketing.Application.Interfaces;

public interface IFlightService
{
    Task<int> CreateFlightAsync(CreateFlightDto dto);
    Task<QueryFlightsResponseDto> QueryFlightsAsync(QueryFlightsRequestDto dto);
    Task<FlightPassengerListResponseDto> GetPassengerListAsync(string flightNumber, DateTime departureDate);
    Task<FlightUploadResponseDto> UploadFlightsAsync(Stream fileStream);
}