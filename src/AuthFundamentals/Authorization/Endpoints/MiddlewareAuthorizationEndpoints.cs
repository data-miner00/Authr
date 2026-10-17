namespace Authorization.Endpoints;

using Microsoft.AspNetCore.Authentication.Cookies;

public static class MiddlewareAuthorizationEndpoints
{
    // Registered separately from the endpoint below - same reasoning as Authentication's
    // ClaimsMiddlewareEndpoints, this must run ahead of any Map* call.
    public static WebApplication UseMiddlewareAuthorization(this WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/middleware"))
            {
                if (!ctx.User.Identities.Any(i => i.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                if (!ctx.User.HasClaim("passport", "eur"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            await next();
        });

        return app;
    }

    public static WebApplication MapMiddlewareAuthorizationEndpoints(this WebApplication app)
    {
        // The check already happened centrally in UseMiddlewareAuthorization - by the time a
        // request reaches here it's already known to be authenticated and authorized.
        app.MapGet("/middleware/secret", () => Results.Ok("allowed"));

        return app;
    }
}
