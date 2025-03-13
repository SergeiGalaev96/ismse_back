using ISMSE_REST_API.Contracts.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Timers;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Models.Delayed;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Services.Scheduler
{
    public class ProcessSchedulerImpl : IProcessScheduler
    {
        private readonly ILogManager _logManager;
        public ProcessSchedulerImpl(ILogManager logManager)
        {
            _logManager = logManager;
        }

        private const double DAILY_24_HOURS_IN_MILLISECONDS = 24 * /*60 * 60 * */1000;
        public Timer RecurredJobInSpecificTime(int hours, int minutes, int seconds, ElapsedEventHandler callback)
        {
            var scheduleTimeUTC = DateTime.Today.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds).ToUniversalTime();
            var currentTimeUTC = DateTime.UtcNow;
            var scheduleIntervalInMs = calcFutureMs(scheduleTimeUTC, currentTimeUTC);
            var timer = RecurredJobAfterInterval(scheduleIntervalInMs, callback);
            ElapsedEventHandler resetAfterFirstOccured = null;
            resetAfterFirstOccured = new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                timer.Interval = DAILY_24_HOURS_IN_MILLISECONDS;
                timer.Elapsed -= resetAfterFirstOccured;
                _logManager.WriteLog("INTERVAL RESETTED to 24 hours in ms");
            });
            timer.Elapsed += resetAfterFirstOccured;

            return timer;
        }

        double calcFutureMs(DateTime scheduleTime, DateTime currentTime)
        {
            var remainingTimeInMs = (scheduleTime - currentTime).TotalMilliseconds;
            if (remainingTimeInMs <= 0)
            {
                return calcFutureMs(scheduleTime.AddDays(1), currentTime);
            }
            return remainingTimeInMs;
        }

        public Timer RecurredJobAfterInterval(double intervalInMs, ElapsedEventHandler callback)
        {
            var timer = new Timer(intervalInMs);
            timer.Elapsed += callback;
            timer.Enabled = true;
            return timer;
        }
    }
}