namespace AuthServer.Controllers;

using AuthServer.Models;
using AuthServer.Models.Private;
using AuthServer.Repositories;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Token([AsParameters] GetTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GrantType) ||
            string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            return this.BadRequest(new FailedResponse
            {
                Error = "invalid_request",
                ErrorDescription = "Missing required parameters.",
            });
        }

        if (request.GrantType != "client_credentials")
        {
            return this.BadRequest(new FailedResponse
            {
                Error = "unsupported_grant_type",
                ErrorDescription = "Only 'client_credentials' grant type is supported.",
            });
        }

        var registration = await this.repository.Get(request.ClientId);

        if (registration is null)
        {
            return this.Unauthorized(new FailedResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Client not registered.",
            });
        }

        if (!registration.ClientSecret.Equals(request.ClientSecret, StringComparison.Ordinal))
        {
            return this.Unauthorized(new FailedResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Invalid client credentials.",
            });
        }

        var token = JwtBuilder.Create()
            .WithAlgorithm(this.algorithm)
            .AddClaim(ClaimName.Issuer, this.jwtOption.Issuer)
            .AddClaim(ClaimName.ExpirationTime, DateTimeOffset.UtcNow.AddMinutes(this.jwtOption.ExpirationInMinutes).ToUnixTimeSeconds())
            .AddClaim(ClaimName.Audience, registration.Name)
            .AddClaim(ClaimName.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .AddClaim(ClaimName.NotBefore, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .AddClaim(ClaimName.Subject, registration.ClientId)
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
