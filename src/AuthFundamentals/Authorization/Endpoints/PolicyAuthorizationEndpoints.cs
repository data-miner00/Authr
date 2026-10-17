namespace Authorization.Endpoints;

public static class PolicyAuthorizationEndpoints
{
    public const string PolicyName = "eu-passport";

    public static WebApplication MapPolicyAuthorizationEndpoints(this WebApplication app)
    {
        app.MapGet("/policy/secret", () => "allowed").RequireAuthorization(PolicyName);

        return app;
    }
}
