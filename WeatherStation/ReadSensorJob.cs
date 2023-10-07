using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Iot.Device.DHTxx;
using Quartz;
using UnitsNet;

namespace WeatherStation;

public class ReadSensorJob : IJob
{
    private const string Bucket = "zimmer";
    private const string Org = "main";
    private const int SensorPin = 4;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var data = (DataCache)context.MergedJobDataMap.Get("data")!;
            var writeApi = (WriteApiAsync)context.MergedJobDataMap.Get("writeApi")!;
            var measurement = ReadData();

            await writeApi.WriteMeasurementAsync(Bucket, Org, WritePrecision.Ms, measurement,
                context.CancellationToken);

            data.CurrentTemperature = measurement.DegreesCelsius;
            data.CurrentHumidity = measurement.PercentHumidity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static EnvironmentMeasurement ReadData()
    {
        var sensor = new Dht11(SensorPin);
        var tempReadSuccessful = false;
        var humidityReadSuccessful = false;

        Temperature temp = default;
        RelativeHumidity humidity = default;
        while (!tempReadSuccessful)
        {
            temp = sensor.Temperature;
            tempReadSuccessful = sensor.IsLastReadSuccessful;
        }

        while (!humidityReadSuccessful)
        {
            humidity = sensor.Humidity;
            humidityReadSuccessful = sensor.IsLastReadSuccessful;
        }

        return new EnvironmentMeasurement
        {
            Timestamp = DateTimeOffset.Now,
            DegreesCelsius = temp.DegreesCelsius,
            PercentHumidity = humidity.Percent,
        };
    }
}