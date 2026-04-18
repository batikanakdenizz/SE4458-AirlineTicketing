namespace AirlineTicketing.Domain.Enums;

public enum BookingStatus
{
    PendingPayment = 1,
    Confirmed = 2,
    Ticketed = 3,
    Expired = 4,
    Cancelled = 5,
    Refunded = 6
}
