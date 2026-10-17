namespace Authentication.Endpoints;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public static class BuiltInCookieEndpoints
{
    public static WebApplication MapBuiltInCookieEndpoints(this WebApplication app)
    {
        // Same outcome as the three stages above, in two lines - AddAuthentication().AddCookie()
        // in Program.cs does the encrypting, the cookie writing and the ClaimsPrincipal wiring.
        app.MapGet("/built-in/login", async (HttpContext ctx) =>
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "sharon")],
                CookieAuthenticationDefaults.AuthenticationScheme);

            await ctx.SignInAsync(new ClaimsPrincipal(identity));
            return "ok";
        });

        app.MapGet("/built-in/whoami", (HttpContext ctx) => ctx.User.Identity!.Name)
            .RequireAuthorization();

        return app;
    }
}
