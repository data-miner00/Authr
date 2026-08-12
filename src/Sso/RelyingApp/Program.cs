namespace RelyingApp;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using RelyingApp.Options;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var sso = builder.Configuration.GetSection("Sso").Get<SsoOptions>()
            ?? throw new InvalidOperationException("Sso configuration section not set.");

        // No Cookie.Domain set here: this app only ever reads the cookie the identity
        // provider wrote to the shared domain, it never issues its own.
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                // Replaces the default "redirect to this app's own /Account/Login" behaviour
                // (which doesn't exist here) with a hop to the identity provider instead.
                o.Events.OnRedirectToLogin = ctx =>
                {
                    var currentUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}";
                    var returnUrl = Uri.EscapeDataString(currentUrl);
                    ctx.Response.Redirect($"{sso.IdentityProviderLoginUrl}?returnUrl={returnUrl}");
                    return Task.CompletedTask;
                };
            });

        builder.Services.AddAuthorization();

        // Same key ring + application name as the identity provider - this is what makes
        // the cookie it issued decryptable here.
        var keyRingPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, sso.KeyRingPath));
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
            .SetApplicationName(sso.KeyRingApplicationName);

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", (HttpContext ctx) =>
            ctx.User.Identity?.IsAuthenticated == true
                ? $"Relying App. Signed in as {ctx.User.Identity.Name}."
                : "Relying App. Not signed in - visit /secret to start SSO.");

        app.MapGet("/secret", (HttpContext ctx) =>
            $"Welcome {ctx.User.Identity!.Name}. This page trusted a cookie it never issued.")
            .RequireAuthorization();

        app.Run();
    }
}
