using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Infrastructure
{
    public interface IConverterPresenter
    {
        DateTime GetDateTime(string src);

        /// <summary>
        /// Parse Hour part from predefined in HH:mm format
        /// </summary>
        /// <param name="timeStr">Input full time expression like 19:45</param>
        /// <returns></returns>
        int GetHours(string timeStr);
        /// <summary>
        /// Parse Minute part from predefined in HH:mm format
        /// </summary>
        /// <param name="timeStr">Input full time expression like 19:45</param>
        /// <returns></returns>
        int GetMinutes(string timeStr);
    }
}
