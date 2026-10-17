namespace Authorization.Endpoints;

using Microsoft.AspNetCore.Authentication.Cookies;

public static class ManualAuthorizationEndpoints
{
    public static WebApplication MapManualAuthorizationEndpoints(this WebApplication app)
    {
        // Authentication and authorization both hand-checked per endpoint - repetitive, but
        // shows exactly what the later stages automate.
        app.MapGet("/manual/secret", (HttpContext ctx) =>
        {
            if (!ctx.User.Identities.Any(i => i.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme))
            {
                return Results.Unauthorized();
            }

            // Results.Forbid() would trigger the cookie scheme's ForbidAsync challenge (a
            // redirect to its AccessDeniedPath) instead of a plain 403 - StatusCode sidesteps it.
            if (!ctx.User.HasClaim("passport", "eur"))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            return Results.Ok("allowed");
        });

        return app;
    }
}
