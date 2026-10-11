using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Common;
using Resto.Api.Data;
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
// Settings (appsettings.json -> "Pricing"), checked at startup
// ---------------------------------------------------------------
builder.Services
    .AddOptions<PricingOptions>()
    .Bind(builder.Configuration.GetSection(PricingOptions.SectionName))
    .Validate(o => o.TaxRate is >= 0 and < 1, "Pricing:TaxRate must be between 0 and 1 (e.g. 0.06 for 6%).")
    .ValidateOnStart();

// ---------------------------------------------------------------
// Application services (business logic)
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
// Enums are sent as text ("Pending") instead of numbers (0).
// ---------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SupportNonNullableReferenceTypes());

// Health check: /health returns "Healthy" when Oracle is reachable
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<RestoDbContext>("oracle");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
