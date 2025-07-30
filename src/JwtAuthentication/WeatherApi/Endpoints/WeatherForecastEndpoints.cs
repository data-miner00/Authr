namespace WeatherApi.Endpoints;

/// <summary>
/// The weather forecast endpoints.
/// </summary>
public static class WeatherForecastEndpoints
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// Initializes the weather forecast endpoints in the application.
    /// </summary>
    /// <param name="app">The application as pointer.</param>
    /// <returns>The application itself.</returns>
    public static WebApplication UseWeatherForecastEndpoints(this WebApplication app)
    {
        app.MapGetWeatherForecastEndpoint();

        return app;
    }

    private static WebApplication MapGetWeatherForecastEndpoint(this WebApplication app)
    {
        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
                .ToArray();
            return Results.Ok(forecast);
        })
            .RequireAuthorization()
            .WithName("GetWeatherForecast");

        return app;
    }
}

/// <summary>
/// The weather forecast record.
/// </summary>
/// <param name="Date">The date of the forecast.</param>
/// <param name="TemperatureC">The temperature.</param>
/// <param name="Summary">The short summary.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1400:Access modifier should be declared", Justification = "It's file scoped.")]
file sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Gets the temperature in Farenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
}
