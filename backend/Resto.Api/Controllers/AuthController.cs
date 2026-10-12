using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;
using Resto.Api.Dtos;
using Resto.Api.Entities;

namespace Resto.Api.Controllers;

/// <summary>
/// Staff login with a secure cookie.
/// The browser stores the cookie (HttpOnly, so JavaScript cannot read it)
/// and sends it automatically with every request.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    RestoDbContext db,
    IPasswordHasher<User> hasher,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]   // max 5 attempts per minute per IP address
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrentUserDto>> Login(LoginRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        var result = user is null
            ? PasswordVerificationResult.Failed
            : hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (user is null || result == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Failed login for '{Username}'", username);
            // Same message whether the username or the password was wrong,
            // so attackers can't find out which usernames exist.
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Login failed",
                detail: "Wrong username or password.");
        }

        // Upgrade old password hashes to the current algorithm settings
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(ct);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        logger.LogInformation("'{Username}' logged in", user.Username);
        return Ok(new CurrentUserDto(user.UserId, user.Username, user.Role));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>Who is logged in? The website calls this when it opens.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        // Re-check the database, in case the account was deleted after logging in
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id, ct);

        if (user is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized();
        }

        return Ok(new CurrentUserDto(user.UserId, user.Username, user.Role));
    }
}
