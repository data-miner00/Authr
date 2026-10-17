namespace Authentication;

using Authentication.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Used by both EncryptedCookieEndpoints and ClaimsMiddlewareEndpoints to protect the
        // cookie value by hand, before the built-in cookie scheme is introduced in stage 4.
        builder.Services.AddDataProtection();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                // Minimal APIs have no /Account/Login page for the default cookie handler to
                // redirect to - without this, an unauthenticated /built-in/whoami 302s to a route
                // that doesn't exist instead of a clean 401.
                o.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseClaimsMiddleware();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () =>
            "AuthFundamentals - Authentication. Stages: /raw-cookie, /encrypted-cookie, /claims-middleware, /built-in.");

        app.MapRawCookieEndpoints();
        app.MapEncryptedCookieEndpoints();
        app.MapClaimsMiddlewareEndpoints();
        app.MapBuiltInCookieEndpoints();

        app.Run();
    }
}
