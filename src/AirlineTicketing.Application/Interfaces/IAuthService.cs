using AirlineTicketing.Application.DTOs.Auth;

namespace AirlineTicketing.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}