using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("default")
    .AddCookie("default", o =>
    {
        o.Cookie.Name = "kookie";
        o.Cookie.Domain = string.Empty;  // Browser will autopopulate to current domain
        // o.Cookie.Path = "/path"; // This cookie will only applicable to the path set
        o.Cookie.HttpOnly = false;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;

        // Need to enable persistent in the login request first
        o.ExpireTimeSpan = TimeSpan.FromSeconds(20);

        // Refresh cookie when the cookie is almost expired
        o.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello world");

app.MapPost("/login", async (HttpContext ctx) =>
{
    var user = new ClaimsPrincipal(
        new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            ],
            "default"));

    await ctx.SignInAsync("default", user, new AuthenticationProperties
    {
        IsPersistent = true,
    });
});

app.MapGet("/test", () => "Protected world").RequireAuthorization();

app.MapGet("/challenge", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync("default",
        new AuthenticationProperties
        {
            RedirectUri = "/challenge-uri",
        });
});

app.MapGet("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("default",
        new AuthenticationProperties
        {
            IsPersistent = true,
        });
});

app.Run();
