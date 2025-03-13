
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Status
{
    public class SetStatusManuallyRequestDTO
    {
        public string statusDate { get; set; }
        public Guid docId { get; set; }
        public Guid stateTypeId { get; set; }
    }
}