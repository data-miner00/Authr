namespace IdentityApi;

using Microsoft.AspNetCore.Identity.Data;
using System.IdentityModel.Tokens.Jwt;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var secretKey = builder.Configuration["SecretKey"] ?? throw new InvalidOperationException("SecretKey not set.");
        builder.Services.AddSingleton(new TokenGenerator(secretKey, new JwtSecurityTokenHandler()));

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapPost("/login", (LoginRequest request, TokenGenerator tokenGenerator) =>
        {
            return Results.Ok(new
            {
                token = tokenGenerator.GenerateToken(Guid.NewGuid(), request.Email),
            });
        });

        app.UseHttpsRedirection();

        app.Run();
    }
}
