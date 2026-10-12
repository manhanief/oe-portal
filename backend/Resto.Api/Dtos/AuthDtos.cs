using Resto.Api.Entities;

namespace Resto.Api.Dtos;

public record LoginRequest(string Username, string Password);

public record CurrentUserDto(int Id, string Username, UserRole Role);
