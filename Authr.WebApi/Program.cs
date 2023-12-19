using Authr.Core;
using Authr.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

const string AuthScheme = "cookie";
const string AuthScheme2 = "cookie2";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configure the HTTP request pipeline.

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();


var summaries = WeatherForecastContext.summaries.ToArray();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecastContext.WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

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
    return "allowed";
});

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>
    {
        new Claim("usr", "sharon"),
        new Claim("passport", "eur"),
        new Claim(ClaimTypes.DateOfBirth, DateTime.Now.AddYears(-19).ToString())
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