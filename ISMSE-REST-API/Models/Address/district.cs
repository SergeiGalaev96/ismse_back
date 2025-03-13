using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Address
{
    public class district
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public Guid? districtType { get; set; }
        public Guid? regionId { get; set; }
    }
}