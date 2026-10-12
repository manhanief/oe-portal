using Resto.Api.Entities;

namespace Resto.Api.Dtos;

// ---------- Categories (CategoryDto is reused for responses) ----------

public record SaveCategoryRequest(string Name, int SortOrder, bool IsActive);

// ---------- Menu items ----------

public record AdminMenuItemDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable);

public record SaveMenuItemRequest(
    int CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable);

// ---------- Tables (TableDto is reused for responses) ----------

public record SaveTableRequest(string TableNo, int Seats);

// ---------- Staff accounts ----------

public record UserDto(int Id, string Username, UserRole Role);

public record CreateUserRequest(string Username, string Password, UserRole Role);

public record ResetPasswordRequest(string Password);

// ---------- Sales report ----------

public record TopItemDto(string Name, int Quantity, decimal Revenue);

public record DailySalesDto(
    DateOnly Date,
    int OrderCount,
    int CancelledCount,
    decimal Revenue,
    decimal AverageOrder,
    IReadOnlyList<TopItemDto> TopItems);
