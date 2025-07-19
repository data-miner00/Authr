namespace AuthServer.Repositories;

using AuthServer.Models.Private;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ScopeRepository : IScopeRepository
{
    private readonly List<ScopeInfo> scopes = [];

    public Task<IList<ScopeInfo>> GetAsync()
    {
        return Task.FromResult<IList<ScopeInfo>>(this.scopes);
    }

    public Task<ScopeInfo?> GetAsync(string scope)
    {
        var scopeInfo = this.scopes.FirstOrDefault(x => x.Scope.Equals(scope));

        return Task.FromResult(scopeInfo);
    }

    public Task RegisterAsync(ScopeInfo scope)
    {
        this.scopes.Add(scope);
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(string scope)
    {
        var scopeInfo = this.scopes.FirstOrDefault(x => x.Scope.Equals(scope));

        if (scopeInfo != null)
        {
            this.scopes.Remove(scopeInfo);
        }

        return Task.CompletedTask;
    }
}
