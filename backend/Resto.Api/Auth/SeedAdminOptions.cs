namespace Resto.Api.Auth;

/// <summary>
/// The first admin account, created at startup when the USERS table is empty.
/// Set with user-secrets (never in appsettings.json):
///   dotnet user-secrets set "SeedAdmin:Username" "admin"
///   dotnet user-secrets set "SeedAdmin:Password" "..."
/// </summary>
public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public string? Username { get; set; }
    public string? Password { get; set; }
}
