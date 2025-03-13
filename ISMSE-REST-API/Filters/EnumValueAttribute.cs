using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Filters
{
    public class EnumValueAttribute : Attribute
    {
        public Guid[] ValueIds { get; private set; }
        public string[] Texts { get; private set; }

        /// <summary>
        /// Custom Enum Item Decorator
        /// </summary>
        /// <param name="ids">Enum Ids - Comma Separated</param>
        /// <param name="texts">Enum Captions - Comma Separated</param>
        public EnumValueAttribute(string ids, string texts)
        {
            ValueIds = ids.Split(',').Select(x => Guid.Parse(x)).ToArray();
            Texts = texts.Split(',');
        }
    }
}