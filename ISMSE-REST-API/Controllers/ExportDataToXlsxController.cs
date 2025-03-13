using ISMSE_REST_API.Contracts.CustomExporter;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Models.CustomExportModels.DTO;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services;
using Raven.Imports.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ExportDataToXlsxController : ApiController
    {
        private readonly IExporterFacade _exporterFacade;
        private readonly IConverterPresenter _converterPresenter;
        private readonly ILogManager _logManager;
        public ExportDataToXlsxController(IExporterFacade exporterFacade, IConverterPresenter converterPresenter, ILogManager logManager)
        {
            _exporterFacade = exporterFacade;
            _converterPresenter = converterPresenter;
            _logManager = logManager;
        }

        [HttpPost]
        public IHttpActionResult ExportGrownByStatus([FromBody] ExportParamsGrownDTO dto)
        {

            if (dto == null || dto.state == null || dto.state.Length == 0)
            {
                _logManager.WriteLog($"{nameof(ExportParamsGrownDTO)}\n{JsonConvert.SerializeObject(dto)}");
                _logManager.WriteLog("Статус не указан!");
                throw new ArgumentNullException("state", "Статус не указан!");
            }
            var state = dto.state;
            byte[] documentDownload = _exporterFacade.GetExcelAsByteArray(
                state: state.Cast<Enum>().ToArray(),
                startDate: _converterPresenter.GetDateTime(dto.startDate),
                endDate: _converterPresenter.GetDateTime(dto.endDate),
                msecId: dto.msecId,
                regionId: dto.regionId,
                districtId: dto.districtId);
            return ResponseMessage(createExcelFileResponse(documentDownload, "ExportGrownMSEC"));
        }

        [HttpPost]
        public IHttpActionResult ExportChildByStatus([FromBody] ExportParamsChildDTO dto)
        {
            if (dto == null || dto.state == null || dto.state.Length == 0)
            {
                _logManager.WriteLog($"{nameof(ExportParamsGrownDTO)}\n{JsonConvert.SerializeObject(dto)}");
                _logManager.WriteLog("Статус не указан!");
                throw new ArgumentNullException("state", "Статус не указан!");
            }
            var state = dto.state;
            byte[] documentDownload = _exporterFacade.GetExcelAsByteArray(
                state: state.Cast<Enum>().ToArray(),
                startDate: _converterPresenter.GetDateTime(dto.startDate),
                endDate: _converterPresenter.GetDateTime(dto.endDate),
                msecId: dto.msecId,
                regionId: dto.regionId,
                districtId: dto.districtId);
            
            return ResponseMessage(createExcelFileResponse(documentDownload, "ExportChildMSEC"));
        }

        private HttpResponseMessage createExcelFileResponse(byte[] data, string fileName)
        {
            HttpResponseMessage result;
            if (data != null)
            {
                var stream = new MemoryStream(data);
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = string.Concat(fileName, ".", "xlsx")
                    };
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.NoContent);
            }
            return result;
        }
    }
}
