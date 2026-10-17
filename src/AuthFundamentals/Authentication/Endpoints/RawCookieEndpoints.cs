namespace Authentication.Endpoints;

public static class RawCookieEndpoints
{
    private const string CookieName = "raw-cookie-auth";

    public static WebApplication MapRawCookieEndpoints(this WebApplication app)
    {
        // Plain text, no scheme, no encryption - the starting point everything else improves on.
        app.MapGet("/raw-cookie/login", (HttpContext ctx) =>
        {
            ctx.Response.Headers.SetCookie = $"{CookieName}=sharon";
            return "ok";
        });

        app.MapGet("/raw-cookie/whoami", (HttpContext ctx) =>
        {
            var cookie = ctx.Request.Headers.Cookie
                .FirstOrDefault(c => c is not null && c.StartsWith($"{CookieName}=", StringComparison.Ordinal));

            return cookie is null ? Results.Unauthorized() : Results.Ok(cookie.Split('=', 2)[1]);
        });

        return app;
    }
}
