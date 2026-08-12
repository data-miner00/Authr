namespace RelyingApp.Options;

/// <summary>
/// Settings shared by convention with <c>IdentityProvider</c> - the key ring must match
/// exactly on both sides for a cookie issued there to be readable here.
/// </summary>
public class SsoOptions
{
    public required string IdentityProviderLoginUrl { get; init; }

    public required string KeyRingPath { get; init; }

    public required string KeyRingApplicationName { get; init; }
}
