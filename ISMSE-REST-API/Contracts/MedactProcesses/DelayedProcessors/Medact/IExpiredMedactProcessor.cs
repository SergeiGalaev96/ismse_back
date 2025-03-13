using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact
{
    public interface IExpiredMedactProcessor
    {
        void Execute();
        Guid[] ChildExpiredEntries { get; }
        Guid[] AdultExpiredEntries { get; }
    }
}
