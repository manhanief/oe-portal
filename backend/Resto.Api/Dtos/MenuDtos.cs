namespace Resto.Api.Dtos;

public record MenuItemDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable);   // false = show as "Sold out" in the UI

public record MenuCategoryDto(
    int Id,
    string Name,
    IReadOnlyList<MenuItemDto> Items);
