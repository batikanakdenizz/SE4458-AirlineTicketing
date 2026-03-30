using AirlineTicketing.Application.DTOs.CheckIn;

namespace AirlineTicketing.Application.Interfaces;

public interface ICheckInService
{
    Task<CheckInResponseDto> CheckInAsync(CheckInRequestDto dto);
}