namespace Authr.Cookie.Schemes;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

/// <summary>
/// Visitor authentication handler.
/// </summary>
/// <param name="options">The cookie auth options.</param>
/// <param name="logger">The logger factory.</param>
/// <param name="encoder">The url encoder.</param>
/// <param name="clock">System clock.</param>
#pragma warning disable CS0618 // Type or member is obsolete
public class VisitorAuthHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
    : CookieAuthenticationHandler(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
{
    /// <inheritdoc/>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Try to authenticate
        var result = await base.HandleAuthenticateAsync();
        if (result.Succeeded)
        {
            return result;
        }

        // If sign in unsuccessful, create a default visitor id
        var claims = new List<Claim>
        {
            new("usr", "sharon"),
        };
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);

        await this.Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
