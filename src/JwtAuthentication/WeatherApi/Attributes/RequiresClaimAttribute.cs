namespace WeatherApi.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class RequiresClaimAttribute : Attribute, IAuthorizationFilter
{
    private readonly string claimName;
    private readonly string claimValue;

    public RequiresClaimAttribute(string claimName, string claimValue)
    {
        this.claimName = claimName;
        this.claimValue = claimValue;
    }

    /// <inheritdoc/>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.HasClaim(this.claimName, this.claimValue))
        {
            context.Result = new ForbidResult();
        }
    }
}
