using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Resto.Api.Data;

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
// API
// Enums are sent as text ("Pending") instead of numbers (0).
// ---------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health check: /health returns "Healthy" when Oracle is reachable
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<RestoDbContext>("oracle");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
