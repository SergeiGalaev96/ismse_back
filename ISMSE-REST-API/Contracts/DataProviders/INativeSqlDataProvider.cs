using ISMSE_REST_API.Models.CustomExportModels;
using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.DataProviders
{
    public interface INativeSqlDataProvider
    {
        List<CustomExportItem[]> FetchData(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null);
    }
}
