namespace AuthServer.Models;

public class RegistrationRequest
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string ClientId { get; set; }

    public string RedirectUri { get; set; }
}
