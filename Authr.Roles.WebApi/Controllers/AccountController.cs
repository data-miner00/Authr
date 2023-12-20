namespace Authr.Roles.WebApi.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The account auth controller.
/// </summary>
[ApiController]
public class AccountController : ControllerBase
{
    /// <summary>
    /// Login to the application.
    /// </summary>
    /// <returns>The Http response.</returns>
    [HttpGet("/login")]
    public IActionResult Login()
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "admin"), // using proper role type
            new("superfluous_role", "admin"), // can use self-defined claim as role that needs to be specified below
        ];

        var identity = new ClaimsIdentity(claims, "cookie", null, roleType: "superfluous_role");

        var principal = new ClaimsPrincipal(identity);

        return this.SignIn(principal, authenticationScheme: "cookie");
    }
}
