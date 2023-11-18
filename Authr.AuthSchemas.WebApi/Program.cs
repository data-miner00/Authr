using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication()
    //.AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", (o) => { })
    .AddCookie("visitor")
    .AddCookie("oauth-cookie")
    .AddCookie("local")
    .AddOAuth("external-oauth", (o) =>
    {
        o.SignInScheme = "oauth-cookie";

        o.ClientId = "id";
        o.ClientSecret = "secret";

        o.AuthorizationEndpoint = "https://oauth.wiremockapi.cloud/oauth/authorize";
        o.TokenEndpoint = "https://oauth.wiremockapi.cloud/oauth/token";
        o.UserInformationEndpoint = "https://oauth.wiremockapi.cloud/userinfo";
        o.CallbackPath = "/cb-oauth";

        o.Scope.Add("profile");
        o.SaveTokens = true;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("customer", pb =>
    {
        pb
            .AddAuthenticationSchemes("local", "visitor", "oauth-cookie")
            .RequireAuthenticatedUser();
    })
    .AddPolicy("user", pb =>
    {
        pb
            .AddAuthenticationSchemes("local")
            .RequireAuthenticatedUser();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.MapGet("/", () => Task.FromResult("hello")).RequireAuthorization("customer");

app.MapGet("/login-local", async (ctx) =>
{
    var claims = new List<Claim>
    {
        new("usr", "anton"),
    };

    var identity = new ClaimsIdentity(claims, "local");
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("local", user);
});

app.MapGet("/login-oauth", async (ctx) =>
{
    await ctx.ChallengeAsync("external-oauth", new AuthenticationProperties
    {
        RedirectUri = "/",
    });
}).RequireAuthorization("user");


app.Run();

#pragma warning disable CS0618 // Type or member is obsolete
public class VisitorAuthHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
    : CookieAuthenticationHandler(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync();

        if (result.Succeeded)
        {
            return result;
        }

        var claims = new List<Claim>
        {
            new("usr", "anton"),
        };
        var identity = new ClaimsIdentity(claims, "visitor");
        var user = new ClaimsPrincipal(identity);

        await Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
