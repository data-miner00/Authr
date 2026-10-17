namespace Authorization.Endpoints;

public static class CustomRequirementAuthorizationEndpoints
{
    public const string PolicyName = "eu-passport-adult";

    public static WebApplication MapCustomRequirementAuthorizationEndpoints(this WebApplication app)
    {
        app.MapGet("/custom-requirement/secret", () => "allowed").RequireAuthorization(PolicyName);

        return app;
    }
}
