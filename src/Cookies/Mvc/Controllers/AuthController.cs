namespace Mvc.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        // Avoid storing so much data in cookie, else it will reach max 4096Bytes and chunked to small pieces
        // This is bad lots of data consume bandwitdh
        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, "shaun"),
            new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
            new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
        ];

        var claimsIdentity = new ClaimsIdentity(claims, "default");

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return this.SignIn(claimsPrincipal, "default");
    }
}
