namespace Authr.IdentityMgmt.WebApi;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

/// <summary>
/// Utility class for user.
/// </summary>
public static class UserHelper
{
    /// <summary>
    /// Convert a <see cref="User"/> to an equivalent <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="user">The user to be converted.</param>
    /// <returns>The converted claims principal.</returns>
    public static ClaimsPrincipal Convert(User user)
    {
        var claims = new List<Claim>
        {
            new("username", user.Username),
        };

        claims.AddRange(user.Claims);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new(identity);
    }
}
