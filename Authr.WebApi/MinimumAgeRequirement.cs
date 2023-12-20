namespace Authr.WebApi;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// The minimum age requirement.
/// </summary>
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    private readonly int minimumAge;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinimumAgeRequirement"/> class.
    /// </summary>
    /// <param name="minimumAge">The minimum age.</param>
    public MinimumAgeRequirement(int minimumAge)
    {
        this.minimumAge = minimumAge;
    }

    /// <summary>
    /// Gets the configured minimum age.
    /// </summary>
    public int MinimumAge => this.minimumAge;
}
