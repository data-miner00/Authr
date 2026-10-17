namespace Authorization.Policies;

using Microsoft.AspNetCore.Authorization;

public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge => minimumAge;
}
