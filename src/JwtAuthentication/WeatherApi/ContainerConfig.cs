namespace WeatherApi;

using Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

/// <summary>
/// The container configuration for the API.
/// </summary>
public static class ContainerConfig
{
    /// <summary>
    /// Configures the web application with the necessary services.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>The web application instance.</returns>
    /// <exception cref="InvalidOperationException">Throw if <see cref="ApplicationOptions"/> is not configured.</exception>
    public static WebApplication Configure(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = builder.Configuration
            .GetSection(nameof(ApplicationOptions))
            .Get<ApplicationOptions>()
            ?? throw new InvalidOperationException("ApplicationOptions not set.");

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey)), // Also can use Convert.FromBase64String
                    ValidateIssuer = true,
                    ValidIssuer = config.Issuer,
                    ValidateAudience = true,
                    ValidAudience = config.Audience,
                    ValidateLifetime = true,
                };
            });

        builder.Services.AddOpenApi();

        return builder.Build();
    }
}
