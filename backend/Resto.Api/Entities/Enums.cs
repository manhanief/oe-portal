namespace Resto.Api.Entities;

// Stored in Oracle as UPPER_SNAKE text, e.g. DineIn -> 'DINE_IN'.
// The conversion is applied automatically in RestoDbContext.

public enum OrderType
{
    DineIn,
    Takeaway
}

public enum OrderStatus
{
    Pending,
    Preparing,
    Ready,
    Completed,
    Cancelled
}

public enum UserRole
{
    Admin,
    Staff
}
