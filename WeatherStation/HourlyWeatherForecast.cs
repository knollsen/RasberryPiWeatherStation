using System;

namespace WeatherStation;

public record HourlyWeatherForecast(DateTimeOffset Time, double Temperature, double Precipitation_probability);