namespace IdentityProvider.Options;

/// <summary>
/// Settings shared by convention with <c>RelyingApp</c> - the cookie domain and key ring
/// must match exactly on both sides for the issued cookie to be usable across apps.
/// </summary>
public class SsoOptions
{
    public required string CookieDomain { get; init; }

    public required string KeyRingPath { get; init; }

    public required string KeyRingApplicationName { get; init; }

    /// <summary>
    /// Only redirect back to a host within our own SSO domain after login, otherwise an
    /// attacker could craft a login link that hands off the session to an arbitrary site.
    /// </summary>
    public bool IsTrustedReturnUrl(string? returnUrl) =>
        Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) &&
        uri.Host.EndsWith(this.CookieDomain.TrimStart('.'), StringComparison.OrdinalIgnoreCase);
}
