namespace Authr.Core
{
    public class WeatherForecast
    {
        private readonly DateOnly date;
        private readonly int temperacureC;
        private readonly string? summary;

        public WeatherForecast(DateOnly date, int temperacureC, string? summary)
        {
            this.date = date;
            this.temperacureC = temperacureC;
            this.summary = summary;
        }

        public static readonly IReadOnlyList<string> Summaries = [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public int TemperatureF => 32 + (int)(this.temperacureC / 0.5566);
    }
}
