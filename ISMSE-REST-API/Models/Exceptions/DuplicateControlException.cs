﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Exceptions
{
    public class DuplicateControlException : Exception
    {
        public DuplicateControlException(string message) : base(message)
        {
        }
    }
}