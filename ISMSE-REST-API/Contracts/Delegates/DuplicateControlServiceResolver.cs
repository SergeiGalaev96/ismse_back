using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Contracts.Delegates
{
    public delegate IDuplicateControl DuplicateControlServiceResolver(string identifier);
}