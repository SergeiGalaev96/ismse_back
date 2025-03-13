using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Exceptions
{
    public class TaskDuplicateException : Exception
    {
        public TaskDuplicateException(string message) : base(message)
        {
        }
    }
}