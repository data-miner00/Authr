namespace IdentityApi.Models;

public class LoginResponse
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}
