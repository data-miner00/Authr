namespace Authr.Roles.IF.Controllers;

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
        var signInResult = await signInManager.PasswordSignInAsync("kinsta", password: Program.ComplicatedPassword, false, false);

        return signInResult.Succeeded ? this.Ok() : this.BadRequest();
    }
}
