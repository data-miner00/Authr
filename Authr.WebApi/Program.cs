using Authr.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

const string AuthScheme = "cookie";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("cookie")
    .AddCookie(AuthScheme);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthentication();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
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
    if (!ctx.User.Identities.Any(x => x.Name == AuthScheme))
    {
        ctx.Response.StatusCode = 401;
        return "denied";
    }

    if (!ctx.User.HasClaim("passport", "eur"))
    {
        ctx.Response.StatusCode = 403;
        return "denied";
    }

    return "allowed";
});

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>
    {
        new Claim("usr", "sharon"),
        new Claim("passport", "eur")
    };
    var identity = new ClaimsIdentity(claims, AuthScheme);
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(AuthScheme, user);
    return "ok";
});

app.MapGet("/login_manual", (HttpContext ctx, IDataProtectionProvider idp) =>
{
    var protector = idp.CreateProtector("auth-cookie");
    ctx.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:sharon")}";
    return "ok";
});

app.Run();

var person = new Person2("Alex", "Jones", DateTime.Now, 180);
var other = person with { FirstName = "Bob" };

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
