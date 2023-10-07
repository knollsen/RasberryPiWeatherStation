using FakeItEasy;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace WeatherStation.Specs
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var cut = new WeatherForecastJob();

            var context = A.Fake<IJobExecutionContext>();

            var data = new DataCache();
            A.CallTo(() => context.MergedJobDataMap.Get("data"))
                .Returns(data);

            await cut.Execute(context);
        }
    }
}