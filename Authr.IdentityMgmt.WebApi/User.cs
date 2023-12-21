namespace Authr.IdentityMgmt.WebApi;

using System.Security.Claims;

/// <summary>
/// The user model.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the hashed password.
    /// </summary>
    public string HashedPassword { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user claims.
    /// </summary>
    public List<Claim> Claims { get; set; } = [];
}
