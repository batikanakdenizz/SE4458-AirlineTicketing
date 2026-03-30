using AirlineTicketing.Application.DTOs.Ticket;

namespace AirlineTicketing.Application.Interfaces;

public interface ITicketService
{
    Task<BuyTicketResponseDto> BuyTicketAsync(BuyTicketRequestDto dto);
}