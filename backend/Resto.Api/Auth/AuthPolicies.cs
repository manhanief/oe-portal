namespace Resto.Api.Auth;

/// <summary>
/// Names of the authorization policies, used like [Authorize(Policy = AuthPolicies.Admin)].
/// </summary>
public static class AuthPolicies
{
    /// <summary>Any logged-in staff member (Staff or Admin): kitchen board.</summary>
    public const string Staff = "Staff";

    /// <summary>Admins only: menu, tables, staff accounts, sales.</summary>
    public const string Admin = "Admin";
}
