using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StaticReferencesController : ApiController
    {
        public IEnumerable<object> GetExportGrownStatuses() => StaticReferences.getSimpleEnumItems<CustomExportAdultState>();
        public IEnumerable<object> GetExportChildStatuses() => StaticReferences.getSimpleEnumItems<CustomExportChildState>();
    }
}
