using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Infrastructure
{
    public interface ILogWriter
    {
        void WriteLog(string msg);
    }
}
