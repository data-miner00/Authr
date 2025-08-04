namespace AuthServer;

using AuthServer.Models.Private;
using AuthServer.Repositories;

internal static class Seeder
{
    public static async Task SeedAsync(IApplicationRepository applicationRepository, IScopeRepository scopeRepository)
    {
        await applicationRepository.SeedApplicationInfo();
        await scopeRepository.SeedScopesAsync();
    }

    private static Task SeedApplicationInfo(this IApplicationRepository repository)
    {
        var now = DateTime.UtcNow;

        var portal = new ApplicationInfo
        {
            Name = "Web Portal",
            Description = "A dummy web portal.",
            ClientId = "web-portal",
            ClientSecrets =
            [
                new ClientSecret
                {
                    Id = Guid.NewGuid().ToString(),
                    Secret = Guid.NewGuid().ToString("N"),
                    CreatedAt = now,
                    ExpiresAt = now.AddYears(1),
                },
                new ClientSecret
                {
                    Id = Guid.NewGuid().ToString(),
                    Secret = Guid.NewGuid().ToString("N"),
                    CreatedAt = now.AddDays(2),
                    ExpiresAt = now.AddYears(1),
                },
            ],
            //RedirectUris = ["https://scalar.com/callback"],
            Scopes = ["read", "write"],
        };

        return repository.RegisterAsync(portal);
    }

    private static async Task SeedScopesAsync(this IScopeRepository repository)
    {
        var scopes = new List<ScopeInfo>
        {
            new() { Scope = "read", Description = "Read access" },
            new() { Scope = "write", Description = "Write access" },
        };

        foreach (var scope in scopes)
        {
            await repository.RegisterAsync(scope);
        }
    }
}
