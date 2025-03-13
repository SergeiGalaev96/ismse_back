using ISMSE_REST_API.Contracts.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Infrastructure
{
    public class LogWriterInMemoryImpl : ILogWriter
    {
        public readonly static ConcurrentBag<string> Logs = new ConcurrentBag<string>();
        static object lockObj = new object();
        public void WriteLog(string msg)
        {
            lock (lockObj)
                Logs.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} : {msg}");
        }
    }
}