using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Authr.IdentityMgmt.WebApi;

public class Database
{
    private static string Hash(string str) =>
        Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(str)));

    public async Task<User?> GetUserAsync(string username)
    {
        var hash = Hash(username);
        if (!File.Exists(hash))
        {
            return null;
        }

        await using var reader = File.OpenRead(hash);
        return await JsonSerializer.DeserializeAsync<User>(reader);
    }

    public async Task PutAsync(User user)
    {
        var hash = Hash(user.Username);
        await using var writer = File.OpenWrite(hash);
        await JsonSerializer.SerializeAsync(writer, user);
    }
}

public class User
{
    public string Username { get; set; }
    public string HashedPassword { get; set; }
    public List<UserClaim> Claims { get; set; } = new();
}

public record UserClaim(string Type, string Value);
