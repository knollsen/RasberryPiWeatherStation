using System;

namespace WeatherStation;

public class TemperatureDatePage : LCDPage
{
    public override void Build(DataCache data)
    {
        var temperature = $"{data.CurrentTemperature}C";
        var date = DateTime.Now.ToString("dd.MM.yyyy");

        this.FirstLine = FormatStringForLcd(temperature, date);

        var humidity = $"{data.CurrentHumidity}% H";
        var time = DateTime.Now.ToString("HH:mm");

        this.SecondLine = FormatStringForLcd(humidity, time);
    }
}