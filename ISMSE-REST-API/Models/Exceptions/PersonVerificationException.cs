using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Exceptions
{
    public class PersonVerificationException : Exception
    {
        public PersonVerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}