namespace AuthServer.Models.Private;

public class JwtOption
{
    public const string JwtSectionName = "Jwt";

    public string Issuer { get; set; }

    public int ExpirationInMinutes { get; set; }
}
