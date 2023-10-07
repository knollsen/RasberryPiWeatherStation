using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherStation
{
    public class DataCache
    {
        public double CurrentTemperature { get; set; }

        public double CurrentHumidity { get; set; }

        public HourlyWeatherForecast CurrentForecast { get; set; }

        public HourlyWeatherForecast ForecastInTwoHours { get; set; }

        public HourlyWeatherForecast ForecastTomorrowMorning { get; set; }
    }
}
