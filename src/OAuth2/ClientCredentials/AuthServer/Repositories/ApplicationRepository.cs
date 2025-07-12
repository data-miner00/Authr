namespace AuthServer.Repositories;

using AuthServer.Models.Private;
using System.Threading.Tasks;

/// <summary>
/// The in-memory application registration repository.
/// </summary>
public class ApplicationRepository : IApplicationRepository
{
    private readonly List<ApplicationInfo> applications = [];

    /// <inheritdoc/>
    Task<ApplicationInfo?> IApplicationRepository.GetAsync(string clientId)
    {
        var application = this.applications.FirstOrDefault(app => app.ClientId == clientId);
        return Task.FromResult(application);
    }

    /// <inheritdoc/>
    Task<bool> IApplicationRepository.IsRegisteredAsync(string clientId)
    {
        return Task.FromResult(this.applications.Exists(app => app.ClientId == clientId));
    }

    /// <inheritdoc/>
    Task IApplicationRepository.RegisterAsync(ApplicationInfo info)
    {
        this.applications.Add(info);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    Task IApplicationRepository.UnregisterAsync(string clientId)
    {
        var app = this.applications.FirstOrDefault(app => app.ClientId == clientId);

        if (app is not null)
        {
            this.applications.Remove(app);
        }

        return Task.CompletedTask;
    }
}
