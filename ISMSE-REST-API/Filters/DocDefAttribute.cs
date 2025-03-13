using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Filters
{
    public class DocDefAttribute : Attribute
    {
        public Guid DefId { get; private set; }
        public string DefText { get; private set; }

        /// <summary>
        /// Custom EnumDef Decorator
        /// </summary>
        /// <param name="defId">EnumDefId</param>
        /// <param name="defText">EnumDefCaption</param>
        public DocDefAttribute(string defId, string defText)
        {
            DefId = Guid.Parse(defId);
            DefText = defText;
        }
    }
}