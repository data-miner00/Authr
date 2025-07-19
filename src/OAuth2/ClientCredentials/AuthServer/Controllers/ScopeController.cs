namespace AuthServer.Controllers;

using AuthServer.Models;
using AuthServer.Models.Private;
using AuthServer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ScopeController : ControllerBase
{
    private readonly IScopeRepository repository;

    public ScopeController(IScopeRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        this.repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var scopes = await this.repository.GetAsync();

        return this.Ok(scopes);
    }

    [HttpGet("{scope}")]
    public async Task<IActionResult> GetAsync(string scope)
    {
        var scopeInfo = await this.repository.GetAsync(scope);

        return scopeInfo is null ? this.NotFound() : this.Ok(scopeInfo);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsync(RegisterScopeRequest request)
    {
        var scope = new ScopeInfo
        {
            Scope = request.Scope,
            Name = request.Name,
            Description = request.Description,
        };

        await this.repository.RegisterAsync(scope);

        return this.Created();
    }

    [HttpDelete("{scope}")]
    public async Task<IActionResult> UnregisterAsync(string scope)
    {
        await this.repository.UnregisterAsync(scope);

        return this.NoContent();
    }
}
