using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models
{
    public class OperationResult
    {
        public bool isSuccess { get; set; }
        public object data { get; set; }
        public string errorMessage { get; set; }

        public static OperationResult Success()
        {
            return new OperationResult { isSuccess = true };
        }
        public static OperationResult Success(object data)
        {
            return new OperationResult { isSuccess = true, data = data };
        }
        public static OperationResult Failure(string errorMessage)
        {
            return new OperationResult { errorMessage = errorMessage, isSuccess = false };
        }
    }
}