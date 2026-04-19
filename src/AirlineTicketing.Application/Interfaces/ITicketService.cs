using AirlineTicketing.Application.DTOs.Ticket;

namespace AirlineTicketing.Application.Interfaces;

public interface ITicketService
{
    Task<BuyTicketResponseDto> BuyTicketAsync(BuyTicketRequestDto dto);
    Task<TicketDetailsResponseDto?> GetTicketAsync(string ticketNumber);
    Task<TicketDetailsResponseDto> CancelTicketAsync(string ticketNumber);
    Task<TicketDetailsResponseDto> BoardTicketAsync(string ticketNumber);
}
