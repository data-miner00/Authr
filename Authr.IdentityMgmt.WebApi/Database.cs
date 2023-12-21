namespace Authr.IdentityMgmt.WebApi;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// A simple implementation of persistence layer for user.
/// </summary>
public class Database
{
    /// <summary>
    /// Get user from file system.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The user found.</returns>
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

    /// <summary>
    /// Upsert a user.
    /// </summary>
    /// <param name="user">The user to be upserted.</param>
    /// <returns>Nothing.</returns>
    public async Task PutAsync(User user)
    {
        var hash = Hash(user.Username);
        await using var writer = File.OpenWrite(hash);
        await JsonSerializer.SerializeAsync(writer, user);
    }

    private static string Hash(string str) =>
        Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(str)));
}
