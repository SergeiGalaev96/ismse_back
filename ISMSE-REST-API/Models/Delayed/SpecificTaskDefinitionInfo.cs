using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace ISMSE_REST_API.Models.Delayed
{
    public class SpecificTaskDefinitionInfo
    {
        public readonly Guid TaskId;
        public readonly string TimerStatus;
        public readonly string[] Tasks;
        public readonly string NextRun;
        public SpecificTaskDefinitionInfo(Timer timer, Task[] tasks, Guid taskId)
        {
            TimerStatus = $"Timer-Enabled: {timer.Enabled}";
            Tasks = tasks.Select(x => $"task-scheduler-Id: {x.Id}, task-scheduler-status:{x.Status}").ToArray();
            TaskId = taskId;
            NextRun = "will calculate later";
        }
    }
}