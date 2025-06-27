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
        if (string.IsNullOrWhiteSpace(grant_type) ||
            string.IsNullOrWhiteSpace(client_id) ||
            string.IsNullOrWhiteSpace(client_secret))
        {
            return this.BadRequest(new FailedResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Missing required parameters.",
            });
        }

        if (grant_type != "client_credentials")
        {
            return this.BadRequest(new FailedResponse
            {
                Error = "unsupported_grant_type",
                ErrorDescription = "Only 'client_credentials' grant type is supported.",
            });
        }

        var registration = await this.repository.Get(client_id);

        if (registration is null)
        {
            return this.Unauthorized(new FailedResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Client not registered.",
            });
        }

        if (!registration.ClientSecret.Equals(client_secret, StringComparison.Ordinal))
        {
            return this.Unauthorized(new FailedResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Invalid client credentials.",
            });
        }

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
}
