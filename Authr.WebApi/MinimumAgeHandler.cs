namespace Authr.WebApi;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// The class that handles minimum age checks.
/// </summary>
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        var dobClaim = context.User.FindFirst(ClaimTypes.DateOfBirth);

        if (dobClaim is null)
        {
            return Task.CompletedTask;
        }

        var dob = Convert.ToDateTime(dobClaim.Value);
        var age = DateTime.Today.Year - dob.Year;

        if (dob > DateTime.Today.AddYears(-age))
        {
            age--;
        }

        if (age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
