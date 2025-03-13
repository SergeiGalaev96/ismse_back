using ISMSE_REST_API.Models.Delayed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact
{
    public interface ISpecificTaskManager
    {
        void RunNewTaskMedactCleaner(int? hours = null, int? minutes = null, int? secs = null);
        SpecificTaskDefinitionInfo[] GetTasks();
        void ClearTasks();
    }
}
