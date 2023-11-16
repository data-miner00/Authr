using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Authr.IdentityFramework.WebApi.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpGet("login")]
        public async Task<IActionResult> Login(SignInManager<IdentityUser> signInManager)
        {
            await signInManager.PasswordSignInAsync("test@test.com", "test", false, false);

            return this.Ok();
        }
    }
}
