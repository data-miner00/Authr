namespace AuthServer.Repositories;

using AuthServer.Models.Private;

public interface IScopeRepository
{
    Task<IList<ScopeInfo>> GetAsync();

    Task<ScopeInfo?> GetAsync(string scope);

    Task RegisterAsync(ScopeInfo scope);

    Task UnregisterAsync(string scope);
}
