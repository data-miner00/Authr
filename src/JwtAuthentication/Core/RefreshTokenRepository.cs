namespace Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class RefreshTokenRepository
{
    private static List<RefreshToken> RefreshTokens { get; } = [];

    public static RefreshToken? GetByToken(string token)
    {
        return RefreshTokens.FirstOrDefault(x => x.Token == token);
    }

    public static void Add(RefreshToken refreshToken)
    {
        RefreshTokens.Add(refreshToken);
    }

    public static void Remove(string userId)
    {
        RefreshTokens.RemoveAll(x => x.UserId == userId);
    }
}
