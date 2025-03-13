using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Models.Enums.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Contracts.Delegates
{
    public delegate ILogWriter LogWriterServiceResolver(LogType logType);
}