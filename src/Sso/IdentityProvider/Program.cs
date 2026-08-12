namespace IdentityProvider;

using System.Security.Claims;
using IdentityProvider.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var sso = builder.Configuration.GetSection("Sso").Get<SsoOptions>()
            ?? throw new InvalidOperationException("Sso configuration section not set.");

        // The cookie domain is scoped to the parent domain, so every app under it
        // receives the cookie too. This one setting is the whole of "single sign-on".
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                o.Cookie.Domain = sso.CookieDomain;
            });

        builder.Services.AddAuthorization();

        // RelyingApp points at this same folder and application name - that's what lets it
        // decrypt a cookie it never issued.
        var keyRingPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, sso.KeyRingPath));
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
            .SetApplicationName(sso.KeyRingApplicationName);

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => "Identity Provider. Sign in at /login.");

        app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, "sharon"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(new ClaimsPrincipal(identity));

            return Results.Redirect(sso.IsTrustedReturnUrl(returnUrl) ? returnUrl! : "/");
        });

        app.MapGet("/logout", async (HttpContext ctx) =>
        {
            // The cookie is shared, so this ends the session for every app on the domain.
            await ctx.SignOutAsync();
            return "Signed out.";
        });

        app.MapGet("/whoami", (HttpContext ctx) =>
            Results.Ok(ctx.User.Claims.Select(c => new { c.Type, c.Value })))
            .RequireAuthorization();

        app.Run();
    }
}
