using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace WeatherStation
{
    public class WeatherForecastJob : IJob
    {
        private static readonly string URL =
            "https://api.open-meteo.com/v1/forecast?latitude=48.236904&longitude=16.348769&hourly=temperature_2m,precipitation_probability&forecast_days=2";

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("Retrieving weather forecast.");

                var client = new HttpClient();

                var weather = await client.GetFromJsonAsync<WeatherForecastDto>(URL);

                var hourly = weather!.BuildHourlyWeatherForecasts().ToList();

                var data = (DataCache)context.MergedJobDataMap.Get("data")!;

                var today = DateTimeOffset.Now.Date;
                var tomorrowMorning = new DateTimeOffset(today).AddHours(8);

                data.CurrentForecast = hourly.MinBy(w => Math.Abs(w.Time.Subtract(DateTimeOffset.Now).TotalSeconds));
                data.ForecastInTwoHours = hourly.MinBy(w => Math.Abs(w.Time.Subtract(DateTimeOffset.Now.Add(TimeSpan.FromHours(2))).TotalSeconds));
                data.ForecastTomorrowMorning = hourly.MinBy(w => Math.Abs(w.Time.Subtract(tomorrowMorning).TotalSeconds));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        public record WeatherForecastDto(
            double Latitude,
            double Longitude,
            double generationtime_ms,
            int utc_offset_Seconds,
            string Timezone,
            string Timezone_Abbrevation,
            double Elevation,
            HourlyUnitsDto Hourly_Units,
            HourlyDto Hourly)
        {
            public IEnumerable<HourlyWeatherForecast> BuildHourlyWeatherForecasts()
            {
                return this.Hourly.Time.Select((t, i) => 
                    new HourlyWeatherForecast(
                        DateTimeOffset.Parse(Hourly.Time[i]), 
                        Hourly.Temperature_2m[i], 
                        Hourly.Precipitation_probability[i]))
                    .ToList();
            }
        }

        public record HourlyUnitsDto(
            string Time = "iso8601",
            string Temperature_2M = "°C",
            string Rain = "mm",
            string CloudCover = "%",
            string Windspeed_10M = "km/h");

        public record HourlyDto(
            string[] Time,
            double[] Temperature_2m,
            double[] Precipitation_probability,
            int[] Cloudcover,
            double[] Windspeed_10m);
    }
}
