using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.CustomExportModels.DTO
{
    public class ExportParamsDTOBase
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
        public Guid? msecId { get; set; }
        public int? regionId { get; set; }
        public int? districtId { get; set; }
    }
    public class ExportParamsGrownDTO : ExportParamsDTOBase
    {
        public CustomExportAdultState[] state { get; set; }
    }
    public class ExportParamsChildDTO : ExportParamsDTOBase
    {
        public CustomExportChildState[] state { get; set; }
    }
}