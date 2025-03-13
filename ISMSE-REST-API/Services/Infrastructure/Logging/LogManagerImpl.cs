using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Models.Enums.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Infrastructure.Logging
{
    public class LogManagerImpl : ILogManager
    {
        private readonly ILogWriter _logWriter;
        public LogManagerImpl(LogWriterServiceResolver logResolver)
        {
            _logWriter = logResolver(_currentLogType);
        }
        public LogType GetCurrentLogType { get { return _currentLogType; } }
        private LogType _currentLogType = LogType.MEMORY;
        public void SetLogType(LogType logType)
        {
            _currentLogType = logType;
        }

        public void WriteLog(string msg)
        {
            _logWriter.WriteLog(msg);
        }
    }
}