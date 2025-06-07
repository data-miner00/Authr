namespace AuthServer.Repositories;

using AuthServer.Models.Private;

public interface IApplicationRepository
{
    Task RegisterAsync(ApplicationInfo info);

    Task<bool> IsRegisteredAsync(string clientId);

    Task UnregisterAsync(string clientId);

    Task<ApplicationInfo?> Get(string clientId);
}
