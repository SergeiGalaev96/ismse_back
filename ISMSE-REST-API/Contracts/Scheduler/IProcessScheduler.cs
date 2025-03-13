using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Models.Delayed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ISMSE_REST_API.Contracts.Scheduler
{
    public interface IProcessScheduler
    {
        Timer RecurredJobInSpecificTime(int hours, int minutes, int seconds, ElapsedEventHandler callback);
        Timer RecurredJobAfterInterval(double intervalInMs, ElapsedEventHandler callback);
    }
}
