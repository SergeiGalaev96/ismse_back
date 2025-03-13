using ISMSE_REST_API.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Infrastructure
{
    public class LogWriterToFileImpl : ILogWriter
    {
        static readonly object lockObj = new object();
        public void WriteLog(string msg)
        {
            var logPath = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["internalLogsFolderPath"], "LogWriterToFile.log");
            using (var sw = new StreamWriter(logPath, true))
            {
                lock (lockObj)
                {
                    sw.WriteLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} : {msg}");
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}