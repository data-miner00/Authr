using Authr.WebApi;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Use((ctx, next) =>
{
    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
    var protector = idp.CreateProtector("auth-cookie");

    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));

    if (authCookie == null)
    {
        return next();
    }

    var protectedPayload = authCookie.Split('=').Last();
    var payload = protector.Unprotect(protectedPayload);
    var parts = payload.Split(':');
    var key = parts[0];
    var value = parts[1];

    var claims = new List<Claim>
    {
        new Claim(key, value)
    };

    var identity = new ClaimsIdentity(claims);
    ctx.User = new ClaimsPrincipal(identity);

    return next();
});

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
    return ctx.User.FindFirst("usr").Value;
});

app.MapGet("/login", (AuthService authService) =>
{
    authService.SignIn();
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
