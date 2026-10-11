namespace Resto.Api.Dtos;

// DTO = Data Transfer Object: the shape the API sends to the frontend.
// We never return EF entities directly, so database changes don't leak into the API.
public record CategoryDto(
    int Id,
    string Name,
    int SortOrder,
    bool IsActive,
    int ItemCount);
