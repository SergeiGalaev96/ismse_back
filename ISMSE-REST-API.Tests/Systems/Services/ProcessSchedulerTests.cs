using FluentAssertions;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Contracts.Scheduler;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Services.Infrastructure;
using ISMSE_REST_API.Services.Scheduler;
using ISMSE_REST_API.Tests.Infrastructure;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class ProcessSchedulerTests : TestUtils
    {
        public ProcessSchedulerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ScheduleDaily_WhenInvoked_StartsTimerFor_DailyTask()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IProcessScheduler>();
            var logManager = _ninjectGetService<ILogManager>();
            int delaySeconds = 3;
            var scheduleTime = DateTime.Now.AddSeconds(delaySeconds);
            _output.WriteLine($"scheduleTime: {scheduleTime:yyyy-MM-dd HH:mm:ss.fff}");
            //Act
            try
            {
                ElapsedEventHandler callback = null;
                var timer = sut.RecurredJobInSpecificTime(scheduleTime.Hour, scheduleTime.Minute, scheduleTime.Second, callback);
                LogWriterInMemoryImpl.Logs.Any(x => x.Contains("INTERVAL RESETTED")).Should().BeFalse();
                Thread.Sleep(TimeSpan.FromSeconds(delaySeconds + 1));

                //Assert
                LogWriterInMemoryImpl.Logs.Any(x => x.Contains("INTERVAL RESETTED")).Should().BeTrue();
                
            }
            finally
            {
                _output.WriteLine(JsonConvert.SerializeObject(LogWriterInMemoryImpl.Logs));
                LogWriterInMemoryImpl.Logs.Clear();
                _ninjectStop();
            }
        }

        [Fact]
        public void SchedulePerSeconds_WhenInvoked_StartsTimerFor_PerSecondTask()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<IProcessScheduler>();
            var logManager = _ninjectGetService<ILogManager>();
            int intervalInMilliseconds = 2 * 1000;
            int amntOfTasks = 5;
            var now = DateTime.Now;
            var customLogMsg = $"log created at {now}";
            ElapsedEventHandler callback = new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                logManager.WriteLog(customLogMsg);
            });
            //Act
            try
            {
                sut.RecurredJobAfterInterval(intervalInMilliseconds, callback);
                Thread.Sleep(amntOfTasks * intervalInMilliseconds + 1000);

                //Assert
                LogWriterInMemoryImpl.Logs.Count(x => x.Contains(customLogMsg)).Should().Be(amntOfTasks);
            }
            finally
            {
                _output.WriteLine(JsonConvert.SerializeObject(LogWriterInMemoryImpl.Logs));
                LogWriterInMemoryImpl.Logs.Clear();
                _ninjectStop();
            }
        }
    }
}
