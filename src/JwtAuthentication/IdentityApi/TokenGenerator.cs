namespace IdentityApi;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenGenerator
{
    private readonly string secretKey;
    private readonly SecurityTokenHandler tokenHandler;

    public TokenGenerator(string secretKey, SecurityTokenHandler tokenHandler)
    {
        this.secretKey = secretKey;
        this.tokenHandler = tokenHandler;
    }

    public string GenerateToken(Guid userId, string email)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
                    {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        SecurityAlgorithms.HmacSha256Signature),
            Issuer = "http://id.localhost.com",
            Audience = "http://localhost.com",
        };

        var token = this.tokenHandler.CreateToken(tokenDescriptor);
        return this.tokenHandler.WriteToken(token);
    }
}
