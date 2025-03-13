using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace ISMSE_REST_API.Models.Delayed
{
    public class SpecificTaskDefinition
    {
        public SpecificTaskType TaskType { get; set; } = SpecificTaskType.TERMINATE_EXPIRED_MEDACTS;
        public readonly Timer TimerObj;
        public readonly Guid TaskId = Guid.NewGuid();
        private readonly List<Task> _tasks = new List<Task>();
        public SpecificTaskDefinition(Timer timer)
        {
            TimerObj = timer;
        }
        public void AddTask(Task task)
        {
            _tasks.Add(task);
        }
        public SpecificTaskDefinitionInfo GetInfo()
        {
            return new SpecificTaskDefinitionInfo(TimerObj, _tasks.ToArray(), TaskId);
        }
    }
}