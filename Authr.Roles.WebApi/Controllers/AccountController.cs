using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authr.Roles.WebApi.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return this.SignIn(new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim("superfluous_role", "admin"),
                    },
                    "cookie",
                    nameType: null,
                    roleType: "superfluous_role"
                    )
                ),
                    authenticationScheme: "cookie");
        }
    }
}
