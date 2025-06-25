namespace AuthServer.Controllers;

using AuthServer.Models;
using AuthServer.Models.Private;
using AuthServer.Repositories;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[Route("oauth")]
[ApiController]
public class OAuth2Controller : ControllerBase
{
    private readonly IApplicationRepository repository;
    private readonly JwtOption jwtOption;
    private readonly RS256Algorithm algorithm;

    public OAuth2Controller(
        IApplicationRepository repository,
        JwtOption jwtOption,
        RS256Algorithm algorithm)
    {
        this.repository = repository;
        this.jwtOption = jwtOption;
        this.algorithm = algorithm;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromForm] string grant_type, [FromForm] string client_id, [FromForm] string client_secret, [FromForm] string scope)
    {
        if (grant_type == "client_credentials" && client_id == "test_client" && client_secret == "test_secret")
        {
            var token = JwtBuilder.Create()
                .WithAlgorithm(this.algorithm)
                .AddClaim("iss", this.jwtOption.Issuer)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddMinutes(this.jwtOption.ExpirationInMinutes).ToUnixTimeSeconds())
                .AddClaim("shaun", "hello")
                .Encode();

            return this.Ok(new SuccessResponse
            {
                AccessToken = token,
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
