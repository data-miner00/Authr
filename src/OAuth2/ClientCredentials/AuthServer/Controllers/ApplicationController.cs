namespace AuthServer.Controllers;

using AuthServer.Models;
using AuthServer.Models.Private;
using AuthServer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/app")]
[ApiController]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationRepository repository;

    public ApplicationController(IApplicationRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
    {
        var app = new ApplicationInfo
        {
            ClientId = request.ClientId,
            ClientSecret = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            //RedirectUris = request.RedirectUris,
            //Scopes = request.Scopes
        };

        await this.repository.RegisterAsync(app);

        return this.Created();
    }

    [HttpGet("info/{clientId}")]
    public async Task<IActionResult> GetInfo(string clientId)
    {
        var app = await this.repository.Get(clientId);
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
            ClientSecret = app.ClientSecret,
        });
    }
}
