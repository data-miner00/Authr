namespace Authr.IdentityFramework.WebApi.Controllers;

using Microsoft.AspNetCore.Identity;
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
    /// <param name="signInManager">The sign in manager.</param>
    /// <returns>The Http response.</returns>
    [HttpGet("login")]
    public async Task<IActionResult> Login(SignInManager<IdentityUser> signInManager)
    {
        await signInManager.PasswordSignInAsync("test@test.com", "test", false, false);

        return this.Ok();
    }
}
