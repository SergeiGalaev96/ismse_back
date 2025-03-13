using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.MedactProcesses
{
    public interface ITransferChildToAdult
    {
        Guid TransferFromChildToAdult(document dto, Guid userId);
    }
}
