namespace Authr.Roles.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The home controller.
/// </summary>
[ApiController]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Publicly accessible route.
    /// </summary>
    /// <returns>The index string.</returns>
    [HttpGet("/")]
#pragma warning disable S3400 // Methods should not return constants
    public string Index() => "index";
#pragma warning restore S3400 // Methods should not return constants

    /// <summary>
    /// Publicly inaccessible route.
    /// Only those who have "superfluous_role" claim of admin can access.
    /// </summary>
    /// <returns>The secret string.</returns>
    [HttpGet("/secret")]
    [Authorize(Roles = "admin")]
#pragma warning disable S3400 // Methods should not return constants
    public string Secret() => "secret";
#pragma warning restore S3400 // Methods should not return constants
}
