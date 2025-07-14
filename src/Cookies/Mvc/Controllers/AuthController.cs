namespace Mvc.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Mvc.Models;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string Username = "admin";
    private const string Password = "admin";

    [HttpPost("login")]
    public async Task<IActionResult> Login([AsParameters] LoginRequest request)
    {
        if (request.Username != Username || request.Password != Password)
        {
            return this.Unauthorized();
        }

        // Avoid storing so much data in cookie, else it will reach max 4096Bytes and chunked to small pieces
        // This is bad lots of data consume bandwitdh
        List<Claim> claims = [
            new(ClaimTypes.NameIdentifier, request.Username),
            new(ClaimTypes.Role, "admin"),
            new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
        ];

        var claimsIdentity = new ClaimsIdentity(claims, "default");

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/Home/Member",
        };

        return this.SignIn(claimsPrincipal, properties, "default");
    }
}
