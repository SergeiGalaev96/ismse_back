using ISMSE_REST_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Extensions
{
    public static class EnumExtensions
    {

        public static string[] GetValueText(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(EnumValueAttribute)) as EnumValueAttribute;
                    if (attr != null)
                    {
                        return attr.Texts;
                    }
                }
            }
            return null;
        }

        public static Guid[] GetValueId(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(EnumValueAttribute)) as EnumValueAttribute;
                    if (attr != null)
                    {
                        return attr.ValueIds;
                    }
                }
            }
            return null;
        }

        public static Guid GetDefId(this Enum value)
        {
            if (Attribute.GetCustomAttribute(value.GetType(),
                             typeof(DocDefAttribute)) is DocDefAttribute attr)
            {
                return attr.DefId;
            }
            throw new ArgumentNullException("EnumDef", "Данному списочному типу не назначен атрибут EnumDef с указанием Id из базы");
        }
    }
}