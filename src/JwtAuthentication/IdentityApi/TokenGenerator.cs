namespace IdentityApi;

using Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenGenerator
{
    private readonly ApplicationOptions options;
    private readonly SecurityTokenHandler tokenHandler;

    public TokenGenerator(ApplicationOptions options, SecurityTokenHandler tokenHandler)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenHandler);

        this.options = options;
        this.tokenHandler = tokenHandler;
    }

    public string GenerateToken(Guid userId, string email)
    {
        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.SecretKey));
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(this.options.TokenExpirationMinutes),
            SigningCredentials = signingCredentials,
            Issuer = this.options.Issuer,
            Audience = this.options.Audience,
        };

        var token = this.tokenHandler.CreateToken(tokenDescriptor);
        return this.tokenHandler.WriteToken(token);
    }
}
