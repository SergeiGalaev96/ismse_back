using ISMSE_REST_API.Contracts.Builders;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.Builders;
using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ISMSE_REST_API.Controllers
{
    public class TestController : ApiController
    {
        private readonly IConverterPresenter _converterPresenter;
        private readonly IExpiredMedactProcessor _expiredMedactProcessor;
        public TestController(IConverterPresenter converterPresenter, IExpiredMedactProcessor expiredMedactProcessor)
        {
            _converterPresenter = converterPresenter;
            _expiredMedactProcessor = expiredMedactProcessor;
        }
        [HttpGet]
        public IHttpActionResult ConverterPresenter()
        {
            var date = _converterPresenter.GetDateTime("2023-01-01");
            return Ok(date.ToString());
        }
        [HttpGet]
        public IHttpActionResult BuildSql()
        {
            Guid developerUserId = new Guid("{DCED7BEA-8A93-4BAF-964B-232E75A758C5}");
            var developerUsername = "d";
            ITSqlQueryBuilder builder = new TSqlQueryBuilderCissaImpl(ScriptExecutor.CreateContext(developerUsername, developerUserId));
            var result = builder.BuildSql(new[] { CustomExportAdultState.APPROVED_AND_REGISTERED }.Cast<Enum>().ToArray(), DateTime.Today, DateTime.Today);
            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult ChildExpiredEntries() => Ok(_expiredMedactProcessor.ChildExpiredEntries.Length);

        [HttpGet]
        public IHttpActionResult AdultExpiredEntries() => Ok(_expiredMedactProcessor.AdultExpiredEntries.Length);

    }
}
