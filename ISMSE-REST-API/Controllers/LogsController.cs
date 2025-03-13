using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Models.Enums.Logging;
using ISMSE_REST_API.Services.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ISMSE_REST_API.Controllers
{
    public class LogsController : ApiController
    {
        private readonly ILogManager _logManager;
        public LogsController(ILogManager logManager)
        {
            _logManager = logManager;
        }
        [HttpGet]
        public IHttpActionResult SetInMemoryType()
        {
            _logManager.SetLogType(LogType.MEMORY);
            return Ok();
        }
        [HttpGet]
        public IHttpActionResult SetFileType()
        {
            _logManager.SetLogType(LogType.FILE);
            return Ok();
        }
        [HttpGet]
        public IHttpActionResult CurrentLogType() => Ok(_logManager.GetCurrentLogType);
        [HttpGet]
        public IHttpActionResult InMemoryLogs() => Ok(LogWriterInMemoryImpl.Logs);
    }
}
