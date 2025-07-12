namespace AuthServer.Repositories;

using AuthServer.Models.Private;

/// <summary>
/// The application registration repository.
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Adds an application into registration.
    /// </summary>
    /// <param name="info">The app info.</param>
    /// <returns>The task.</returns>
    Task RegisterAsync(ApplicationInfo info);

    /// <summary>
    /// Checks if an app with client Id has been registered.
    /// </summary>
    /// <param name="clientId">The client Id.</param>
    /// <returns>A flag indicating whether the client Id has been registered.</returns>
    Task<bool> IsRegisteredAsync(string clientId);

    /// <summary>
    /// Removes an application registration from the list.
    /// </summary>
    /// <param name="clientId">The client Id.</param>
    /// <returns>The task.</returns>
    Task UnregisterAsync(string clientId);

    /// <summary>
    /// Retrieves an app registration from client Id.
    /// </summary>
    /// <param name="clientId">The client Id.</param>
    /// <returns>The app info.</returns>
    Task<ApplicationInfo?> GetAsync(string clientId);
}
