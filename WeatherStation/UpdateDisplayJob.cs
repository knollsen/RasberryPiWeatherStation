using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace WeatherStation;

public class UpdateDisplayJob : IJob
{
    private const int BacklightActivatedFrom = 8;
    private const int BacklightActivatedTo = 21;
    private const int SwitchInterval = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Console.WriteLine("Updating display...");

            var data = (DataCache)context.MergedJobDataMap.Get("data")!;
            var lcd = (LCDController)context.MergedJobDataMap.Get("lcd")!;
            var pages = ((IEnumerable<LCDPage>)context.MergedJobDataMap.Get("pages")).ToList();

            var currentHour = DateTime.Now.Hour;

            // switch on or off backlight
            lcd.Backlight = currentHour is >= BacklightActivatedFrom and < BacklightActivatedTo;

            var page = GetCurrentPage(pages);
            page.Build(data);

            lcd.WriteToFirstLine(page.FirstLine);
            lcd.WriteToSecondLine(page.SecondLine);

            Console.WriteLine("Updated display.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private LCDPage GetCurrentPage(List<LCDPage> pages)
    {
        var numPages = pages.Count * SwitchInterval;

        var seconds = DateTimeOffset.Now.ToUnixTimeSeconds();

        var index = (seconds % numPages) / SwitchInterval;

        Console.WriteLine("Page with index " + index + " chosen.");

        return pages[(int)index];
    }
}