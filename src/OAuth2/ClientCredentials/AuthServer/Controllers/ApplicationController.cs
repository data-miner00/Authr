namespace AuthServer.Controllers;

using AuthServer.Models;
using AuthServer.Models.Private;
using AuthServer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The application registration controller.
/// </summary>
[Route("api/app")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationRepository appRepository;
    private readonly IScopeRepository scopeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationController"/> class.
    /// </summary>
    /// <param name="appRepository">The application repository.</param>
    /// <param name="scopeRepository">The scope repository.</param>
    public ApplicationController(
        IApplicationRepository appRepository,
        IScopeRepository scopeRepository)
    {
        this.appRepository = appRepository;
        this.scopeRepository = scopeRepository;
    }

    /// <summary>
    /// Registers an application.
    /// </summary>
    /// <param name="request">The app registration request.</param>
    /// <returns>The action result.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        var availableScopes = (await this.scopeRepository.GetAsync()).Select(x => x.Scope);
        var requestedScopesRecognized = request.Scopes.Intersect(availableScopes).Count() == request.Scopes.Count;

        if (!requestedScopesRecognized)
        {
            return this.BadRequest(new FailedResponse
            {
                Error = "not_found",
                ErrorDescription = "One of the scope provided not found.",
            });
        }

        var app = new ApplicationInfo
        {
            ClientId = request.ClientId,
            ClientSecrets = [],
            Name = request.Name,
            Description = request.Description,
            //RedirectUris = request.RedirectUris,
            Scopes = request.Scopes,
        };

        await this.appRepository.RegisterAsync(app);

        return this.Created();
    }

    /// <summary>
    /// Gets the single app registration by client Id.
    /// </summary>
    /// <param name="clientId">The client Id.</param>
    /// <returns>The action result.</returns>
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetInfo(string clientId)
    {
        var app = await this.appRepository.GetAsync(clientId);
        if (app is null)
        {
            return this.NotFound(new FailedResponse
            {
                Error = "not_found",
                ErrorDescription = "Application not found.",
            });
        }

        return this.Ok(new ApplicationInfo
        {
            ClientId = app.ClientId,
            Name = app.Name,
            Description = app.Description,
            ClientSecrets = app.ClientSecrets,
            Scopes = app.Scopes,
        });
    }

    [HttpPost("{clientId}/secret")]
    public async Task<IActionResult> AddSecret(string clientId)
    {
        var app = await this.appRepository.GetAsync(clientId);
        if (app is null)
        {
            return this.NotFound(new FailedResponse
            {
                Error = "not_found",
                ErrorDescription = "Application not found.",
            });
        }

        var secret = new ClientSecret
        {
            Id = Guid.NewGuid().ToString(),
            Secret = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
        };

        app.ClientSecrets.Add(secret);

        return this.Ok(secret);
    }

    [HttpDelete("{clientId}/secret/{secretId}")]
    public async Task<IActionResult> AddSecret(string clientId, string secretId)
    {
        var app = await this.appRepository.GetAsync(clientId);
        if (app is null)
        {
            return this.NotFound(new FailedResponse
            {
                Error = "not_found",
                ErrorDescription = "Application not found.",
            });
        }

        var secret = app.ClientSecrets.Find(x => x.Id == secretId);

        if (secret is null)
        {
            return this.NotFound(new FailedResponse
            {
                Error = "not_found",
                ErrorDescription = "Secret not found.",
            });
        }

        app.ClientSecrets.Remove(secret);

        return this.NoContent();
    }
}
