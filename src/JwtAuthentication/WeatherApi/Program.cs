namespace WeatherApi;

using WeatherApi.Endpoints;
using Scalar.AspNetCore;

public static class Program
{
    public static void Main(string[] args)
    {
        var app = ContainerConfig.Configure(args);

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWeatherForecastEndpoints();
        app.Run();
    }
}
