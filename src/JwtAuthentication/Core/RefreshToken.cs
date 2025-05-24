namespace Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public sealed class RefreshToken
{
    public string Id { get; set; }

    public string Token { get; set; }

    public string UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public static string Generate()
    {
        var randomBytes = new byte[32];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
