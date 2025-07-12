namespace AuthServer.Models.Private;

/// <summary>
/// The application registration information.
/// </summary>
public class ApplicationInfo
{
    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the client Id.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; }
}
