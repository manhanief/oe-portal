using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers.Admin;

/// <summary>Admin: manage staff accounts. Passwords are only ever stored hashed.</summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthPolicies.Admin)]
public class AdminUsersController(RestoDbContext db, IPasswordHasher<User> hasher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken ct) =>
        Ok(await db.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.UserId, u.Username, u.Role))
            .ToListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            throw new BusinessRuleException($"The username '{username}' is already taken.");

        var user = new User { Username = username, Role = request.Role };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Ok(new UserDto(user.UserId, user.Username, user.Role));
    }

    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct)
            ?? throw new NotFoundException($"User {id} was not found.");

        user.PasswordHash = hasher.HashPassword(user, request.Password);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct)
            ?? throw new NotFoundException($"User {id} was not found.");

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (user.UserId == currentUserId)
            throw new BusinessRuleException("You can't delete your own account.");

        if (user.Role == UserRole.Admin &&
            await db.Users.CountAsync(u => u.Role == UserRole.Admin, ct) == 1)
            throw new BusinessRuleException("You can't delete the last admin account.");

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
