using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;

namespace WeatherStation
{
    public class LCDController : IDisposable
    {
        private I2cDevice i2c;
        private Pcf8574 driver;
        private Lcd1602 lcd;

        private bool _backlight;
        public bool Backlight
        {
            get => this._backlight;
            set
            {
                this._backlight = value;
                this.lcd.BacklightOn = value;
            }
        }

        public string CurrentFirstLine { get; private set; } = string.Empty;
        public string CurrentSecondLine { get; private set; } = string.Empty;
        private readonly Mutex mutex = new();

        public void Init()
        {
            mutex.WaitOne();

            this.i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            this.driver = new Pcf8574(i2c);
            this.lcd = new Lcd1602(registerSelectPin: 0,
                enablePin: 2,
                dataPins: new int[] { 4, 5, 6, 7 },
                backlightPin: 3,
                backlightBrightness: 0.2f,
                readWritePin: 1,
                controller: new GpioController(PinNumberingScheme.Logical, driver));

            this.Backlight = true;
            lcd.Clear();

            mutex.ReleaseMutex();
        }

        public void Clear()
        {
            mutex.WaitOne();

            this.CurrentFirstLine = string.Empty;
            this.CurrentSecondLine = string.Empty;

            lcd.Clear();

            mutex.ReleaseMutex();
        }

        public void WriteToFirstLine(string content)
        {
            mutex.WaitOne();

            if (content.Length > 41)
            {
                throw new ArgumentException("Writing strings with over 41 characters results in overflow to second line");
            }

            var indices = this.GetDifferingIndices(this.CurrentFirstLine, content);

            foreach (var index in indices)
            {
                this.lcd.SetCursorPosition(index, 0);

                this.lcd.Write(content[index].ToString());
            }

            this.CurrentFirstLine = content;

            mutex.ReleaseMutex();
        }

        public void WriteToSecondLine(string content)
        {
            mutex.WaitOne();

            var indices = this.GetDifferingIndices(this.CurrentSecondLine, content);

            foreach (var index in indices)
            {
                this.lcd.SetCursorPosition(index, 1);

                this.lcd.Write(content[index].ToString());
            }

            this.CurrentSecondLine = content;

            mutex.ReleaseMutex();
        }

        public void Dispose()
        {
            this.lcd.Dispose();
            this.driver.Dispose();
            this.i2c.Dispose();
            mutex.Dispose();
        }

        private IEnumerable<int> GetDifferingIndices(string currentContent, string newContent)
        {
            var minIndex = newContent.Length < currentContent.Length ? newContent.Length : currentContent.Length;

            var result = new List<int>();
            int i;
            for (i = 0; i < minIndex; i++)
            {
                if (newContent[i] != currentContent[i])
                {
                    result.Add(i);
                }
            }

            if (newContent.Length > currentContent.Length)
            {
                for (; i < newContent.Length; i++)
                {
                    result.Add(i);
                }
            }

            if (currentContent.Length > newContent.Length)
            {
                for (; i < currentContent.Length; i++)
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}
