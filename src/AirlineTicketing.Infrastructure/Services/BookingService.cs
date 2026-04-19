using AirlineTicketing.Application.DTOs.Booking;
using AirlineTicketing.Application.Interfaces;
using AirlineTicketing.Domain.Entities;
using AirlineTicketing.Domain.Enums;
using AirlineTicketing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Services;

public class BookingService : IBookingService
{
    private const int HoldMinutes = 15;
    private const int PageSizeLimit = 10;
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto, string? idempotencyKey)
    {
        ValidateCreateBooking(dto);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Payment)
                .Include(b => b.Passengers)
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey);

            if (existing is not null)
            {
                return ToResponse(existing);
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var (dateStart, dateEnd) = GetUtcDateWindow(dto.DepartureDate);

        var flight = await _context.Flights
            .OrderBy(f => f.DepartureTime)
            .FirstOrDefaultAsync(f =>
                f.FlightNumber == dto.FlightNumber &&
                f.DepartureTime >= dateStart &&
                f.DepartureTime < dateEnd);

        if (flight is null)
        {
            throw new InvalidOperationException("Flight not found.");
        }

        if (flight.Status is FlightStatus.Cancelled or FlightStatus.Departed or FlightStatus.Arrived)
        {
            throw new InvalidOperationException("This flight is not available for booking.");
        }

        var passengerCount = dto.Passengers.Count;
        var updatedRows = await _context.Flights
            .Where(f => f.Id == flight.Id && f.AvailableSeats >= passengerCount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(f => f.AvailableSeats, f => f.AvailableSeats - passengerCount));

        if (updatedRows == 0)
        {
            throw new InvalidOperationException("Sold out or insufficient seats.");
        }

        var booking = new Booking
        {
            PnrCode = await GenerateUniquePnrAsync(),
            FlightId = flight.Id,
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
            ContactEmail = dto.ContactEmail.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            Status = BookingStatus.Ticketed,
            ExpiresAt = DateTime.UtcNow.AddMinutes(HoldMinutes),
            TotalAmount = dto.TotalAmount > 0 ? dto.TotalAmount : passengerCount * 1000m,
            Currency = NormalizeCurrency(dto.Currency),
            CreatedAt = DateTime.UtcNow,
            ConfirmedAt = DateTime.UtcNow
        };

        foreach (var passengerDto in dto.Passengers)
        {
            var passenger = new Passenger
            {
                FirstName = passengerDto.FirstName.Trim(),
                LastName = passengerDto.LastName.Trim(),
                DateOfBirth = passengerDto.DateOfBirth.HasValue ? EnsureUtc(passengerDto.DateOfBirth.Value.Date) : null,
                DocumentNumber = passengerDto.DocumentNumber?.Trim(),
                Nationality = passengerDto.Nationality?.Trim().ToUpperInvariant()
            };

            booking.Passengers.Add(passenger);

            booking.Tickets.Add(new Ticket
            {
                TicketNumber = GenerateTicketNumber(),
                FlightId = flight.Id,
                Passenger = passenger,
                PassengerName = $"{passenger.FirstName} {passenger.LastName}",
                PurchaseDate = DateTime.UtcNow,
                Status = TicketStatus.Issued
            });
        }

        booking.Payment = new Payment
        {
            Provider = "DemoPayment",
            ProviderReference = $"PAY-{Guid.NewGuid():N}"[..24].ToUpperInvariant(),
            Amount = booking.TotalAmount,
            Currency = booking.Currency,
            Status = PaymentStatus.Captured,
            CreatedAt = DateTime.UtcNow,
            CapturedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException) when (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await transaction.RollbackAsync();
            DetachAddedEntities();

            var existing = await FindByIdempotencyKeyAsync(idempotencyKey);
            if (existing is not null)
            {
                return ToResponse(existing);
            }

            throw;
        }

        await transaction.CommitAsync();

        await _context.Entry(booking).Reference(b => b.Flight).LoadAsync();

        return ToResponse(booking);
    }

    public async Task<BookingResponseDto?> GetBookingAsync(string pnrCode)
    {
        if (string.IsNullOrWhiteSpace(pnrCode))
        {
            throw new ArgumentException("PNR code is required.");
        }

        var booking = await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Payment)
            .Include(b => b.Passengers)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.PnrCode == pnrCode.Trim().ToUpperInvariant());

        return booking is null ? null : ToResponse(booking);
    }

    public async Task<BookingListResponseDto> SearchBookingsAsync(BookingSearchRequestDto dto)
    {
        var page = dto.Page;
        var size = dto.Size;
        NormalizePaging(ref page, ref size);

        var query = _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.ContactEmail))
        {
            var email = dto.ContactEmail.Trim().ToLowerInvariant();
            query = query.Where(b => b.ContactEmail.ToLower() == email);
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<BookingStatus>(dto.Status, true, out var status))
            {
                throw new ArgumentException("Invalid booking status.");
            }

            query = query.Where(b => b.Status == status);
        }

        if (dto.DepartureDateFrom.HasValue)
        {
            var from = EnsureUtc(dto.DepartureDateFrom.Value);
            query = query.Where(b => b.Flight.DepartureTime >= from);
        }

        if (dto.DepartureDateTo.HasValue)
        {
            var to = EnsureUtc(dto.DepartureDateTo.Value);
            query = query.Where(b => b.Flight.DepartureTime <= to);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(b => new BookingListItemDto
            {
                PnrCode = b.PnrCode,
                Status = b.Status.ToString(),
                FlightNumber = b.Flight.FlightNumber,
                DepartureTime = b.Flight.DepartureTime,
                ContactEmail = b.ContactEmail,
                PassengerCount = b.Passengers.Count,
                TotalAmount = b.TotalAmount,
                Currency = b.Currency,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return new BookingListResponseDto
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)size)
        };
    }

    public async Task<BookingResponseDto> UpdateContactAsync(string pnrCode, UpdateBookingContactRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ContactEmail) || !dto.ContactEmail.Contains('@'))
        {
            throw new ArgumentException("A valid contact email is required.");
        }

        var booking = await LoadBookingAsync(pnrCode);
        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Refunded or BookingStatus.Expired)
        {
            throw new InvalidOperationException("Contact information cannot be changed for inactive bookings.");
        }

        booking.ContactEmail = dto.ContactEmail.Trim();
        booking.ContactPhone = dto.ContactPhone?.Trim();
        await _context.SaveChangesAsync();

        return ToResponse(booking);
    }

    public async Task<CancelBookingResponseDto> CancelBookingAsync(string pnrCode, CancelBookingRequestDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var booking = await LoadBookingAsync(pnrCode);
        if (booking is null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Refunded)
        {
            return new CancelBookingResponseDto
            {
                PnrCode = booking.PnrCode,
                BookingStatus = booking.Status.ToString(),
                PaymentStatus = booking.Payment?.Status.ToString() ?? PaymentStatus.Pending.ToString(),
                ReleasedSeats = 0,
                Message = "Booking is already inactive."
            };
        }

        if (booking.Flight.Status is FlightStatus.Departed or FlightStatus.Arrived)
        {
            throw new InvalidOperationException("Departed or arrived flights cannot be cancelled through booking management.");
        }

        if (booking.Tickets.Any(t => t.Status is TicketStatus.CheckedIn or TicketStatus.Boarded or TicketStatus.Flown))
        {
            throw new InvalidOperationException("Checked-in or boarded passengers must be offloaded before booking cancellation.");
        }

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT "Id" FROM "Flights" WHERE "Id" = {booking.FlightId} FOR UPDATE""");

        var releasedSeats = booking.Tickets.Count(t => t.Status is TicketStatus.Issued or TicketStatus.Purchased);
        booking.Flight.AvailableSeats = Math.Min(booking.Flight.Capacity, booking.Flight.AvailableSeats + releasedSeats);
        booking.Status = dto.RefundPayment ? BookingStatus.Refunded : BookingStatus.Cancelled;

        foreach (var ticket in booking.Tickets)
        {
            if (ticket.Status is TicketStatus.Issued or TicketStatus.Purchased)
            {
                ticket.Status = dto.RefundPayment ? TicketStatus.Refunded : TicketStatus.Cancelled;
            }
        }

        if (booking.Payment is not null && dto.RefundPayment)
        {
            booking.Payment.Status = PaymentStatus.Refunded;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CancelBookingResponseDto
        {
            PnrCode = booking.PnrCode,
            BookingStatus = booking.Status.ToString(),
            PaymentStatus = booking.Payment?.Status.ToString() ?? PaymentStatus.Pending.ToString(),
            ReleasedSeats = releasedSeats,
            Message = dto.RefundPayment ? "Booking cancelled and payment refunded." : "Booking cancelled."
        };
    }

    private static void ValidateCreateBooking(CreateBookingRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FlightNumber))
            throw new ArgumentException("Flight number is required.");

        if (string.IsNullOrWhiteSpace(dto.ContactEmail) || !dto.ContactEmail.Contains('@'))
            throw new ArgumentException("A valid contact email is required.");

        if (dto.Passengers.Count == 0)
            throw new ArgumentException("At least one passenger is required.");

        if (dto.Passengers.Count > 9)
            throw new ArgumentException("A single booking can contain at most 9 passengers.");

        foreach (var passenger in dto.Passengers)
        {
            if (string.IsNullOrWhiteSpace(passenger.FirstName) || string.IsNullOrWhiteSpace(passenger.LastName))
                throw new ArgumentException("Passenger first name and last name are required.");
        }
    }

    private async Task<string> GenerateUniquePnrAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var pnr = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var exists = await _context.Bookings.AnyAsync(b => b.PnrCode == pnr);
            if (!exists)
                return pnr;
        }

        throw new InvalidOperationException("Could not generate a unique PNR code.");
    }

    private static BookingResponseDto ToResponse(Booking booking)
    {
        return new BookingResponseDto
        {
            PnrCode = booking.PnrCode,
            Status = booking.Status.ToString(),
            FlightNumber = booking.Flight.FlightNumber,
            DepartureTime = booking.Flight.DepartureTime,
            TotalAmount = booking.TotalAmount,
            Currency = booking.Currency,
            PaymentStatus = booking.Payment?.Status.ToString() ?? PaymentStatus.Pending.ToString(),
            ExpiresAt = booking.ExpiresAt,
            Passengers = booking.Passengers
                .OrderBy(p => p.Id)
                .Select(p => new PassengerResponseDto
                {
                    PassengerId = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName
                })
                .ToList(),
            Tickets = booking.Tickets
                .OrderBy(t => t.Id)
                .Select(t => new TicketSummaryDto
                {
                    TicketNumber = t.TicketNumber,
                    PassengerName = t.PassengerName,
                    Status = t.Status.ToString()
                })
                .ToList()
        };
    }

    private static string GenerateTicketNumber()
    {
        return $"TKT-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
    }

    private static string NormalizeCurrency(string currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "TRY"
            : currency.Trim().ToUpperInvariant()[..Math.Min(3, currency.Trim().Length)];
    }

    private async Task<Booking?> FindByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Payment)
            .Include(b => b.Passengers)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey.Trim());
    }

    private async Task<Booking?> LoadBookingAsync(string pnrCode)
    {
        if (string.IsNullOrWhiteSpace(pnrCode))
        {
            throw new ArgumentException("PNR code is required.");
        }

        var normalizedPnr = pnrCode.Trim().ToUpperInvariant();

        return await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Payment)
            .Include(b => b.Passengers)
            .Include(b => b.Tickets)
            .ThenInclude(t => t.CheckIn)
            .FirstOrDefaultAsync(b => b.PnrCode == normalizedPnr);
    }

    private void DetachAddedEntities()
    {
        foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            entry.State = EntityState.Detached;
        }
    }

    private static (DateTime Start, DateTime End) GetUtcDateWindow(DateTime value)
    {
        var utc = EnsureUtc(value);
        var start = DateTime.SpecifyKind(utc.Date, DateTimeKind.Utc);
        return (start, start.AddDays(1));
    }

    private static void NormalizePaging(ref int page, ref int size)
    {
        if (page <= 0) page = 1;
        if (size <= 0) size = PageSizeLimit;
        if (size > PageSizeLimit) size = PageSizeLimit;
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
