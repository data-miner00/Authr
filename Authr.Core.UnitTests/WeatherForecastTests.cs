namespace Authr.Core.UnitTests;

public class WeatherForecastTests
{
    [Fact]
    public void TemperatureF_CalculatedCorrectly()
    {
        const int Temperature = 30;

        var weather = new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now),
            Temperature,
            WeatherForecast.Summaries[0]);

        var expectedF = 85;

        Assert.Equal(expectedF, weather.TemperatureF);
    }
}
