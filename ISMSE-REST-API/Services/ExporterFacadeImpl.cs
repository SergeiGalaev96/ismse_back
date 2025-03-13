using ISMSE_REST_API.Contracts.CustomExporter;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.CustomExporter;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services
{
    public class ExporterFacadeImpl : IExporterFacade
    {
        private readonly ICustomExporter _customExporter;
        public ExporterFacadeImpl(ICustomExporter customExporter)
        {
            _customExporter = customExporter;
        }
        public byte[] GetExcelAsByteArray(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null)
        {
            var data = _customExporter.GetData(state, startDate, endDate, msecId, regionId, districtId);
            return _customExporter.ConvertToFileInByteArray(data);
        }
    }
}