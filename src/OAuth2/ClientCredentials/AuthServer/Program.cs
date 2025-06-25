namespace AuthServer;

using AuthServer.Models.Private;
using AuthServer.Repositories;
using JWT.Algorithms;
using System.Security.Cryptography;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton<IApplicationRepository, ApplicationRepository>();
        var jwt = builder.Configuration.GetSection(JwtOption.JwtSectionName).Get<JwtOption>()
            ?? throw new InvalidOperationException("Jwt section not found");
        builder.Services.AddSingleton(jwt);
        builder.AddRsaAlgorithm();

        var app = builder.Build();

        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static WebApplicationBuilder AddRsaAlgorithm(this WebApplicationBuilder builder)
    {
        var @public = RSA.Create();
        @public.ImportFromPem(KeysHelper.PublicKey());
        var @private = RSA.Create();
        @private.ImportFromPem(KeysHelper.PrivateKey());
        var algorithm = new RS256Algorithm(@public, @private);

        builder.Services.AddSingleton(algorithm);

        return builder;
    }
}
