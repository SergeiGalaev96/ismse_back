using ISMSE_REST_API.Models.Enums.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Infrastructure.Logging
{
    public interface ILogManager
    {
        void SetLogType(LogType logType);
        LogType GetCurrentLogType { get; }
        void WriteLog(string msg);
    }
}
