namespace Authentication.Endpoints;

using Microsoft.AspNetCore.DataProtection;

public static class EncryptedCookieEndpoints
{
    private const string CookieName = "encrypted-cookie-auth";
    private const string ProtectorPurpose = "encrypted-cookie-auth";

    public static WebApplication MapEncryptedCookieEndpoints(this WebApplication app)
    {
        app.MapGet("/encrypted-cookie/login", (HttpContext ctx, IDataProtectionProvider idp) =>
        {
            var protector = idp.CreateProtector(ProtectorPurpose);
            ctx.Response.Headers.SetCookie = $"{CookieName}={protector.Protect("sharon")}";
            return "ok";
        });

        app.MapGet("/encrypted-cookie/whoami", (HttpContext ctx, IDataProtectionProvider idp) =>
        {
            var cookie = ctx.Request.Cookies[CookieName];
            if (cookie is null)
            {
                return Results.Unauthorized();
            }

            var protector = idp.CreateProtector(ProtectorPurpose);
            return Results.Ok(protector.Unprotect(cookie));
        });

        return app;
    }
}
