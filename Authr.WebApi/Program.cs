using System.Security.Claims;
using Authr.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

const string AuthScheme = "cookie";

const string AuthScheme2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie(AuthScheme)
    .AddCookie(AuthScheme2);

builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("eu passport", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthScheme)
            .AddRequirements(new MinimumAgeRequirement(18))
            .RequireClaim("passport", "eur");
    });
});

var app = builder.Build();

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) =>
{
    return ctx.User.FindFirst("usr").Value ?? "empty";
});

app.MapGet("/sweden", (HttpContext ctx) =>
{
    return "allowed";
}).RequireAuthorization("eu passport");

app.MapGet("/norway", (HttpContext ctx) =>
{
    return "allowed";
});

app.MapGet("/denmark", (HttpContext ctx) =>
{
    if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthScheme))
    {
        ctx.Response.StatusCode = 401;
        return string.Empty;
    }

    if (!ctx.User.HasClaim("passport", "eur"))
    {
        ctx.Response.StatusCode = 403;
        return "Forbidden";
    }

    return "allowed";
});

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>
    {
        new("usr", "sharon"),
        new("passport", "eur"),
        new(ClaimTypes.DateOfBirth, DateTime.Now.AddYears(-19).ToString()),
    };

    var identity = new ClaimsIdentity(claims, AuthScheme);
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(AuthScheme, user);
    return "ok";
}).AllowAnonymous();

app.MapGet("/login_manual", (HttpContext ctx, IDataProtectionProvider idp) =>
{
    var protector = idp.CreateProtector("auth-cookie");
    ctx.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:sharon")}";
    return "ok";
});

app.Run();
