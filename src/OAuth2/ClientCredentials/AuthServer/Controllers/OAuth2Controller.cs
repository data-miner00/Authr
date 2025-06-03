namespace AuthServer.Controllers;

using AuthServer.Models;
using Microsoft.AspNetCore.Mvc;

[Route("oauth")]
[ApiController]
public class OAuth2Controller : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> Token([FromForm] string grant_type, [FromForm] string client_id, [FromForm] string client_secret, [FromForm] string scope)
    {
        if (grant_type == "client_credentials" && client_id == "test_client" && client_secret == "test_secret")
        {
            return this.Ok(new SuccessResponse
            {
                AccessToken = "dummy_access_token",
                TokenType = "Bearer",
                ExpiresIn = 3600,
            });
        }

        var failedResponse = new FailedResponse
        {
            Error = "invalid_grant",
            ErrorDescription = "Invalid client credentials or grant type.",
        };

        return this.BadRequest(failedResponse);
    }
}
