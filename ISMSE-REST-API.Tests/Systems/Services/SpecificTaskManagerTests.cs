using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models.Exceptions;
using ISMSE_REST_API.Services.Infrastructure;
using ISMSE_REST_API.Tests.Infrastructure;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ISMSE_REST_API.Tests.Systems.Services
{
    [Collection("Sequential")]
    public class SpecificTaskManagerTests : TestUtils
    {
        public SpecificTaskManagerTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void RunNewTaskMedactCleaner_WhenCalled_CreatesNewDailyTask()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<ISpecificTaskManager>();
            int delaySeconds = 3;
            var scheduleTime = DateTime.Now.AddSeconds(delaySeconds);

            //Act
            try
            {
                sut.RunNewTaskMedactCleaner(scheduleTime.Hour, scheduleTime.Minute, scheduleTime.Second);
                //Assert
                var tasks = sut.GetTasks();
                Thread.Sleep(TimeSpan.FromSeconds(delaySeconds + 5));
                var tasksAfter5Secs = sut.GetTasks();
                _output.WriteLine($"tasks: {JsonConvert.SerializeObject(tasks)}");
                _output.WriteLine($"tasksAfter5Secs: {JsonConvert.SerializeObject(tasksAfter5Secs)}");
            }
            finally
            {
                _output.WriteLine($"logs: {JsonConvert.SerializeObject(LogWriterInMemoryImpl.Logs)}");
                LogWriterInMemoryImpl.Logs.Clear();
                sut.ClearTasks();
                _ninjectStop();
            }
        }

        [Fact]
        public void RunNewTaskMedactCleaner_WhenCalled_ThrowsExistingException()
        {
            //Arrange
            _ninjectStart();
            var sut = _ninjectGetService<ISpecificTaskManager>();


            //Act
            try
            {
                sut.RunNewTaskMedactCleaner();
                //Assert
                Assert.Throws<TaskDuplicateException>(() => sut.RunNewTaskMedactCleaner());
                var tasks = sut.GetTasks();
                Thread.Sleep(TimeSpan.FromSeconds(5));
                var tasksAfter5Secs = sut.GetTasks();
                _output.WriteLine($"tasks: {JsonConvert.SerializeObject(tasks)}");
                _output.WriteLine($"tasksAfter5Secs: {JsonConvert.SerializeObject(tasksAfter5Secs)}");
            }
            finally
            {
                _output.WriteLine($"logs: {JsonConvert.SerializeObject(LogWriterInMemoryImpl.Logs)}");
                LogWriterInMemoryImpl.Logs.Clear();
                sut.ClearTasks();
                _ninjectStop();
            }
        }
    }
}
