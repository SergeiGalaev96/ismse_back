using ISMSE_REST_API.Models.CustomExportModels;
using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.CustomExporter
{
    public interface ICustomExporter
    {
        List<CustomExportItem[]> GetData(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null);
        byte[] ConvertToFileInByteArray(List<CustomExportItem[]> data);
    }
}
