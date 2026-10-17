namespace Authorization;

using System.Security.Claims;
using Authorization.Endpoints;
using Authorization.Policies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
            {
                // Minimal APIs have no /Account/Login or /Account/AccessDenied page for the
                // default cookie handler to redirect to - without this, a failed check 302s to a
                // route that doesn't exist instead of a clean 401/403.
                o.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                o.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(PolicyAuthorizationEndpoints.PolicyName, pb => pb
                .RequireAuthenticatedUser()
                .RequireClaim("passport", "eur"))
            .AddPolicy(CustomRequirementAuthorizationEndpoints.PolicyName, pb => pb
                .RequireAuthenticatedUser()
                .AddRequirements(new MinimumAgeRequirement(18))
                .RequireClaim("passport", "eur"));

        var app = builder.Build();

        // UseAuthentication must run first so ctx.User is populated by the time the
        // middleware-authorization check below inspects it.
        app.UseAuthentication();
        app.UseMiddlewareAuthorization();
        app.UseAuthorization();

        app.MapGet("/", () =>
            "AuthFundamentals - Authorization. Stages: /manual, /middleware, /policy, /custom-requirement.");

        // Shared by every stage below - authorization is the point here, not authentication.
        app.MapGet("/login", async (HttpContext ctx) =>
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "sharon"),
                new("passport", "eur"),
                new(ClaimTypes.DateOfBirth, DateTime.Today.AddYears(-19).ToString("O")),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(new ClaimsPrincipal(identity));
            return "ok";
        });

        app.MapManualAuthorizationEndpoints();
        app.MapMiddlewareAuthorizationEndpoints();
        app.MapPolicyAuthorizationEndpoints();
        app.MapCustomRequirementAuthorizationEndpoints();

        app.Run();
    }
}
