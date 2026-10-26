using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Auth;
using Resto.Api.Common;
using Resto.Api.Data;
using Resto.Api.Entities;
using Resto.Api.Services;
using Resto.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// Database
// The connection string is NOT in appsettings.json (no passwords in code).
// In development it comes from user-secrets; in Docker from an environment variable.
// ---------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Oracle")
    ?? throw new InvalidOperationException(
        "Connection string 'Oracle' is missing. Set it with: dotnet user-secrets set \"ConnectionStrings:Oracle\" \"...\"");

builder.Services.AddDbContext<RestoDbContext>(options =>
    options.UseOracle(connectionString));

// ---------------------------------------------------------------
// Settings, checked at startup
// ---------------------------------------------------------------
builder.Services
    .AddOptions<PricingOptions>()
    .Bind(builder.Configuration.GetSection(PricingOptions.SectionName))
    .Validate(o => o.TaxRate is >= 0 and < 1, "Pricing:TaxRate must be between 0 and 1 (e.g. 0.06 for 6%).")
    .ValidateOnStart();

builder.Services
    .AddOptions<SeedAdminOptions>()
    .Bind(builder.Configuration.GetSection(SeedAdminOptions.SectionName));

// ---------------------------------------------------------------
// Login: secure cookie (HttpOnly, SameSite=Strict)
// ---------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "resto_auth";
        options.Cookie.HttpOnly = true;                       // JavaScript can't read it
        options.Cookie.SameSite = SameSiteMode.Strict;        // not sent from other websites (CSRF protection)
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS-only when served over HTTPS
        options.ExpireTimeSpan = TimeSpan.FromHours(12);      // one long shift
        options.SlidingExpiration = true;

        // This is an API: answer 401/403 instead of redirecting to a login page
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Keys that encrypt the login cookie. In Docker they are saved to a volume,
// otherwise every API restart would log all staff out.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services
        .AddDataProtection()
        .SetApplicationName("Resto")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.Staff, p => p.RequireRole(nameof(UserRole.Staff), nameof(UserRole.Admin)))
    .AddPolicy(AuthPolicies.Admin, p => p.RequireRole(nameof(UserRole.Admin)));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Behind nginx (Docker), use the visitor's real IP address from X-Forwarded-For,
// so the login rate limit applies per visitor instead of to nginx itself.
// Safe here because the API is only reachable through nginx on the internal Docker network.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Slow down password guessing: max 5 login attempts per minute per IP address
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

// ---------------------------------------------------------------
// Application services (business logic) and validation
// ---------------------------------------------------------------
builder.Services.AddScoped<OrderNumberGenerator>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// ---------------------------------------------------------------
// Error handling: every error becomes a clean JSON "ProblemDetails" response
// ---------------------------------------------------------------
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------------------------------------------------------------
// API
// - ValidationFilter runs FluentValidation on every request body automatically
// - Enums are sent as text ("Pending") instead of numbers (0)
// ---------------------------------------------------------------
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SupportNonNullableReferenceTypes());

// Health check: /health returns "Healthy" when Oracle is reachable
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<RestoDbContext>("oracle");

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Create the first admin account if there are no users yet
await AdminSeeder.SeedAsync(app.Services);

app.Run();
