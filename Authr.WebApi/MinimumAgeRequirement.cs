using Microsoft.AspNetCore.Authorization;

namespace Authr.WebApi
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        private readonly int minimumAge;

        public MinimumAgeRequirement(int minimumAge)
        {
            this.minimumAge = minimumAge;
        }

        public int MinimumAge => minimumAge;
    }
}
