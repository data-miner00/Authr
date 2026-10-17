namespace Authentication.Endpoints;

using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;

public static class ClaimsMiddlewareEndpoints
{
    private const string CookieName = "claims-middleware-auth";
    private const string ProtectorPurpose = "claims-middleware-auth";
    private const string AuthenticationType = "claims-middleware";

    // Registered separately from the endpoints below so Program.cs can wire it in with the other
    // pipeline middleware (before UseAuthentication/UseAuthorization), ahead of any Map* call -
    // this is what the built-in cookie scheme does for you automatically in BuiltInCookieEndpoints.
    public static WebApplication UseClaimsMiddleware(this WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/claims-middleware"))
            {
                var cookie = ctx.Request.Cookies[CookieName];
                if (cookie is not null)
                {
                    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
                    var username = idp.CreateProtector(ProtectorPurpose).Unprotect(cookie);
                    var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, username)], AuthenticationType);
                    ctx.User = new ClaimsPrincipal(identity);
                }
            }

            await next();
        });

        return app;
    }

    public static WebApplication MapClaimsMiddlewareEndpoints(this WebApplication app)
    {
        app.MapGet("/claims-middleware/login", (HttpContext ctx, IDataProtectionProvider idp) =>
        {
            var protector = idp.CreateProtector(ProtectorPurpose);
            ctx.Response.Headers.SetCookie = $"{CookieName}={protector.Protect("sharon")}";
            return "ok";
        });

        app.MapGet("/claims-middleware/whoami", (HttpContext ctx) =>
        {
            var name = ctx.User.FindFirst(ClaimTypes.Name)?.Value;
            return name is null ? Results.Unauthorized() : Results.Ok(name);
        });

        return app;
    }
}
