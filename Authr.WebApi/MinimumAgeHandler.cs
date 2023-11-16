using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Authr.WebApi
{
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        /// <summary>
        /// Can use injected services.
        /// </summary>
        public MinimumAgeHandler() { }

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
}
