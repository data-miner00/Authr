namespace AuthServer.Models.Private;

public class ClientSecret
{
    public string Id { get; set; }

    public string Secret { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
