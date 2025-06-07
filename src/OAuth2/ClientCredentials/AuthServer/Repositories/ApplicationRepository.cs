namespace AuthServer.Repositories;

using AuthServer.Models.Private;
using System.Threading.Tasks;

public class ApplicationRepository : IApplicationRepository
{
    private readonly List<ApplicationInfo> applications = [];

    Task<ApplicationInfo?> IApplicationRepository.Get(string clientId)
    {
        var application = this.applications.FirstOrDefault(app => app.ClientId == clientId);
        return Task.FromResult(application);
    }

    Task<bool> IApplicationRepository.IsRegisteredAsync(string clientId)
    {
        return Task.FromResult(this.applications.Exists(app => app.ClientId == clientId));
    }

    Task IApplicationRepository.RegisterAsync(ApplicationInfo info)
    {
        this.applications.Add(info);

        return Task.CompletedTask;
    }

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
