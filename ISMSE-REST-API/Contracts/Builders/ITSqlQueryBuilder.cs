using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Builders
{
    public interface ITSqlQueryBuilder
    {
        string BuildSql(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null);
    }
}
