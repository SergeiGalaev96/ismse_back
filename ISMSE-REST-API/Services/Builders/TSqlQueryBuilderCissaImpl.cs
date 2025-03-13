using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using ISMSE_REST_API.Contracts.Builders;
using ISMSE_REST_API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Builders
{
    public class TSqlQueryBuilderCissaImpl : ITSqlQueryBuilder
    {
        private readonly WorkflowContext _context;
        public TSqlQueryBuilderCissaImpl(WorkflowContext context)
        {
            _context = context;
        }
        private readonly Guid personDefId = new Guid("6052978a-1ecb-4f96-a16b-93548936afc0");
        public string BuildSql(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null)
        {
            var defId = state[0].GetDefId();

            var qb = new QueryBuilder(defId);
            qb.Where("Date").Ge(startDate).And("Date").Le(endDate);
            var query = _context.CreateSqlQuery(qb);
            if (msecId != null)
                query.AndCondition("MseName", ConditionOperation.Equal, msecId);
            if(regionId != null)
                query.AndCondition("Region", ConditionOperation.Equal, regionId);
            if (districtId != null)
                query.AndCondition("District", ConditionOperation.Equal, districtId);

            var docDefRepo = _context.DocDefs;

            foreach (var item in docDefRepo.DocDefById(defId).Attributes)
            {
                if(item.DocDefType == null)
                query.AddAttribute(item.Name).Alias = item.Caption;
            }

            query.TopNo = 10;

            /*
            var script = query.BuildSqlScript(true);
            script.AddSelect("")*/


            return query.BuildSql().ToString();
        }
    }
}