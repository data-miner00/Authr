using System.Security.Claims;
using Authr.Cookie.Schemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", (o) => { })
    //.AddCookie("visitor")
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
        o.CallbackPath = "/cb";

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

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.MapGet("/", () => "hello").RequireAuthorization("customer");

app.MapGet("/login-local", async Task (ctx) =>
{
    var claims = new List<Claim>
    {
        new("usr", "anton"),
    };

    var identity = new ClaimsIdentity(claims, "local");
    var user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("local", user);
});

app.MapGet("/login-oauth", async Task (ctx) =>
{
    await ctx.ChallengeAsync("external-oauth", new AuthenticationProperties
    {
        RedirectUri = "/",
    });
}).RequireAuthorization("user");

app.Run();
