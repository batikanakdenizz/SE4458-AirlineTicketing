using AirlineTicketing.Application.DTOs.Flight;

namespace AirlineTicketing.Application.Interfaces;

public interface IFlightService
{
    Task<int> CreateFlightAsync(CreateFlightDto dto);
    Task<QueryFlightsResponseDto> QueryFlightsAsync(QueryFlightsRequestDto dto);
    Task<FlightPassengerListResponseDto> GetPassengerListAsync(
        string flightNumber,
        DateTime departureDate,
        int page = 1,
        int size = 10);
    Task<FlightUploadResponseDto> UploadFlightsAsync(Stream fileStream);
    Task<FlightDetailsResponseDto?> GetFlightDetailsAsync(string flightNumber, DateTime departureDate);
    Task<FlightDetailsResponseDto> UpdateFlightStatusAsync(string flightNumber, DateTime departureDate, UpdateFlightStatusRequestDto dto);
    Task<FlightDetailsResponseDto> DelayFlightAsync(string flightNumber, DateTime departureDate, DelayFlightRequestDto dto);
}
