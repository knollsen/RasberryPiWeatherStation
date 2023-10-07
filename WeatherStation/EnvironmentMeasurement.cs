using System;
using InfluxDB.Client.Core;

namespace WeatherStation
{
    [Measurement("env")]
    public class EnvironmentMeasurement
    {
        [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }

        [Column("Temperature")] public double DegreesCelsius { get; set; }

        [Column("Humidity")] public double PercentHumidity { get; set; }
    }
}