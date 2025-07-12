namespace Authr.Core;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

/// <summary>
/// The sample implementation of authentication service.
/// </summary>
public class AuthenticationService
{
    private readonly IDataProtectionProvider idp;
    private readonly IHttpContextAccessor accessor;
    private readonly IDataProtector protector;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="idp">The <see cref="IDataProtectionProvider"/>.</param>
    /// <param name="accessor">The <see cref="IHttpContextAccessor"/>.</param>
    public AuthenticationService(IDataProtectionProvider idp, IHttpContextAccessor accessor)
    {
        this.idp = idp;
        this.accessor = accessor;
        this.protector = this.idp.CreateProtector("auth-cookie");
    }

    /// <summary>
    /// Simple sign in method that sets authentication cookies.
    /// </summary>
    public void SignIn()
    {
        this.accessor.HttpContext!.Response.Headers.SetCookie = $"auth={this.protector.Protect("usr:sharon")}";
    }

    /// <summary>
    /// Decrypts username from authentication cookies.
    /// </summary>
    /// <returns>The decrypted username.</returns>
    public string GetUsername()
    {
        var authCookie = this.accessor.HttpContext?.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
        var protectedPayload = authCookie?.Split("=").Last();
        var payload = this.protector.Unprotect(protectedPayload ?? string.Empty);
        var parts = payload.Split(":");
        var value = parts[1];

        return value;
    }
}
