using System;

namespace WeatherStation;

public class CurrentWeatherPage : LCDPage
{
    public override void Build(DataCache data)
    {
        var forecast = data.CurrentForecast;

        if (forecast == null)
        {
            Console.WriteLine("No current forecast available.");

            this.FirstLine = "No current forec";
            this.SecondLine = "ast available   ";

            return;
        }

        this.FirstLine = FormatStringForLcd(forecast.Precipitation_probability + "% Regen", "Jetzt");
        this.SecondLine = FormatStringForLcd(forecast.Temperature + "C aussen", "");
    }
}

public class In2HoursWeatherPage : LCDPage
{
    public override void Build(DataCache data)
    {
        var forecast = data.ForecastInTwoHours;

        if (forecast == null)
        {
            Console.WriteLine("No current forecast available.");

            this.FirstLine = "No current forec";
            this.SecondLine = "ast available   ";

            return;
        }

        this.FirstLine = FormatStringForLcd(forecast.Precipitation_probability + "% Regen", "In 2h");
        this.SecondLine = FormatStringForLcd(forecast.Temperature + "C aussen", "");
    }
}

public class TomorrowMorningWeatherPage : LCDPage
{
    public override void Build(DataCache data)
    {
        var forecast = data.ForecastTomorrowMorning;

        if (forecast == null)
        {
            Console.WriteLine("No current forecast available.");

            this.FirstLine = "No current forec";
            this.SecondLine = "ast available   ";

            return;
        }

        this.FirstLine = FormatStringForLcd(forecast.Precipitation_probability + "% Regen", "Morgen");
        this.SecondLine = FormatStringForLcd(forecast.Temperature + "C aussen", "");
    }
}