namespace Authr.WebApi;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// The program implementation for auths.
/// </summary>
public static class Program
{
    private const string AuthScheme = "cookie";
    private const string AuthScheme2 = "cookie2";

    /// <summary>
    /// The entry of the program.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.RegisterCustomAuth();

        var app = builder.Build();

        app
            .UseHttpsRedirection()
            .UseAuthentication()
            .UseAuthorization();

        app.UseCustomEndpoints();

        app.Run();
    }

    private static WebApplicationBuilder RegisterCustomAuth(this WebApplicationBuilder builder)
    {
        // An authentication can have multiple schemes registered.
        builder.Services.AddAuthentication("cookie")
            .AddCookie(AuthScheme)
            .AddCookie(AuthScheme2);

        builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("eu passport", pb =>
            {
                pb.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AuthScheme)
                    .AddRequirements(new MinimumAgeRequirement(18))
                    .RequireClaim("passport", "eur");
            });

        return builder;
    }

    private static WebApplication UseCustomEndpoints(this WebApplication app)
    {
        // We can use the FindFirst to get the first claim's value that matches the provided key
        app.MapGet("/username", (HttpContext ctx, IDataProtectionProvider idp) =>
        {
            return ctx.User.FindFirst("usr")?.Value ?? "empty";
        });

        // Access to /sweden endpoint requires the caller to fulfill the 'eu passport' policy.
        // In this case it must be in the 'cookie' scheme, more than 18 years old and holds the 'eur' passport,
        // three distinct conditions.
        app.MapGet("/sweden", (HttpContext ctx) =>
        {
            return "allowed";
        }).RequireAuthorization("eu passport");

        // Access to /norway is allowed for everybody.
        app.MapGet("/norway", (HttpContext ctx) =>
        {
            return "allowed";
        });

        // Access to /denmark is also publicly allowed but it actually handles the auth logics manually
        // inside the function, which is not ideal and hard to maintain.
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

        // The login endpoint
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

        // The manual login endpoint that assigns `set-cookie` header manually.
        app.MapGet("/login_manual", (HttpContext ctx, IDataProtectionProvider idp) =>
        {
            var protector = idp.CreateProtector("auth-cookie");

            // equivalent to `ctx.Response.Headers.SetCookie` property
            ctx.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:sharon")}";
            return "ok";
        });

        return app;
    }
}
