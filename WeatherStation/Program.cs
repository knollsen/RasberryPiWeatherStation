using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using InfluxDB.Client;
using Iot.Device.Bmxx80;
using Quartz;
using Quartz.Impl;
using Task = System.Threading.Tasks.Task;

namespace WeatherStation
{
    public class Program
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        private const string Token = "t5mbBLA9EAkLA20LRFsIcZnAoX8pQm4FsLwudX8FXg7QO1zo41fFfm-gKcJSA2N8O6e6cZs_hBV513JwzyQUXQ==";

        private const int ButtonPin = 5;
        private static DateTimeOffset LastButtonPress = DateTimeOffset.MinValue;
        private const int DebounceInSeconds = 1;

        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Started WeatherStation.");

                using var lcd = new LCDController();
                lcd.Init();

                Console.WriteLine("Initialized LCD display.");

                using var client = InfluxDBClientFactory.Create("http://robinknoll.eu:8086", Token.ToCharArray());
                var writeApi = client.GetWriteApiAsync();

                Console.WriteLine("Initialized Influx connection.");

                var factory = new StdSchedulerFactory();
                var scheduler = await factory.GetScheduler();

                var data = new DataCache();
                var pages = ConfigureLCDPages();

                await ScheduleReadSensorJobAsync(scheduler, data, writeApi);

                await ScheduleUpdateDisplayJobAsync(scheduler, data, lcd, pages);

                await ScheduleWeatherForecastJobAsync(scheduler, data);

                await scheduler.Start();

                Console.WriteLine("Scheduled Quartz jobs.");

                RegisterButtonPressAction();

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task ScheduleWeatherForecastJobAsync(IScheduler scheduler, DataCache data)
        {
            var jobKey = new JobKey("weatherforecast");
            var weatherForecastJob = JobBuilder.Create<WeatherForecastJob>()
                .SetJobData(new JobDataMap()
                {
                    { "data", data }
                })
                .WithIdentity(jobKey)
                .Build();

            var weatherForecastTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("0 0 * * * ?")
                .Build();

            await scheduler.ScheduleJob(weatherForecastJob, weatherForecastTrigger);

            await scheduler.TriggerJob(jobKey);
        }

        private static IEnumerable<LCDPage> ConfigureLCDPages()
        {
            return new List<LCDPage>() { new TemperatureDatePage(), new CurrentWeatherPage(), new In2HoursWeatherPage(), new TomorrowMorningWeatherPage() };
        }

        private static async Task ScheduleUpdateDisplayJobAsync(IScheduler scheduler, DataCache data, LCDController lcd, IEnumerable<LCDPage> pages)
        {
            var backlightJob = JobBuilder.Create<UpdateDisplayJob>()
                .SetJobData(new JobDataMap()
                {
                    { "data", data },
                    { "lcd", lcd },
                    { "pages", pages }
                })
                .Build();

            var backlightTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule("* * * * * ?")
                .Build();

            await scheduler.ScheduleJob(backlightJob, backlightTrigger);
        }

        private static async Task ScheduleReadSensorJobAsync(IScheduler scheduler, DataCache data, WriteApiAsync writeApi)
        {
            var dataJob = JobBuilder.Create<ReadSensorJob>()
                .SetJobData(new JobDataMap()
                {
                    { "data", data },
                    { "writeApi", writeApi }
                })
                .Build();

            var dataTrigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(Interval).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(dataJob, dataTrigger);
        }

        private static void RegisterButtonPressAction()
        {
            using var controller = new GpioController();

            controller.OpenPin(ButtonPin);
            controller.SetPinMode(ButtonPin, PinMode.InputPullUp);

            controller.RegisterCallbackForPinValueChangedEvent(ButtonPin, PinEventTypes.Rising, ButtonPress);
        }

        private static void ButtonPress(object sender, PinValueChangedEventArgs args)
        {
            if (LastButtonPress < DateTimeOffset.Now - TimeSpan.FromSeconds(DebounceInSeconds))
            {
                // lcd.Backlight = !lcd.Backlight;

                LastButtonPress = DateTimeOffset.Now;
            }
            
        }
    }
}
