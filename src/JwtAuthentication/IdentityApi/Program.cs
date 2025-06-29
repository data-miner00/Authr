namespace IdentityApi;

using Core;
using IdentityApi.Models;
using Microsoft.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using Scalar.AspNetCore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var config = builder.Configuration
            .GetSection(nameof(ApplicationOptions))
            .Get<ApplicationOptions>()
            ?? throw new InvalidOperationException("ApplicationOptions not set.");

        builder.Services.AddSingleton(new TokenGenerator(config, new JwtSecurityTokenHandler()));

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.MapPost("/login", (LoginRequest request, TokenGenerator tokenGenerator) =>
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.Email,
                Token = RefreshToken.Generate(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            RefreshTokenRepository.Add(refreshToken);

            var loginResponse = new LoginResponse
            {
                AccessToken = tokenGenerator.GenerateToken(Guid.NewGuid(), request.Email),
                RefreshToken = refreshToken.Token,
            };

            return Results.Ok(loginResponse);
        });

        app.MapPost("/login-refresh", (LoginWithRefreshTokenRequest request, TokenGenerator tokenGenerator) =>
        {
            var refreshToken = RefreshTokenRepository.GetByToken(request.RefreshToken);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Results.Unauthorized();
            }

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = refreshToken.UserId,
                Token = RefreshToken.Generate(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            RefreshTokenRepository.Remove(refreshToken.UserId);
            RefreshTokenRepository.Add(newRefreshToken);

            var loginResponse = new LoginResponse
            {
                AccessToken = tokenGenerator.GenerateToken(Guid.NewGuid(), refreshToken.UserId),
                RefreshToken = newRefreshToken.Token,
            };

            return Results.Ok(loginResponse);
        })
        .WithTags("refresh");

        app.MapDelete("/users/{userId}/login-refresh", (string userId) =>
        {
            RefreshTokenRepository.Remove(userId);

            return Results.NoContent();
        })
        .WithTags("refresh");

        app.Run();
    }
}
