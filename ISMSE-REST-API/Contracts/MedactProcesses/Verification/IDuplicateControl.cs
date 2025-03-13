using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.MedactProcesses.Verification
{
    public interface IDuplicateControl
    {
        void VerifyExisting(Guid personId);
    }
}
