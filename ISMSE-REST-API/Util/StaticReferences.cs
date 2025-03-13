using ISMSE_REST_API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ISMSE_REST_API.Util
{
    public class StaticReferences
    {
        public static IEnumerable<object> getSimpleEnumItems<T>() where T : Enum
        {
            var items = (T[])Enum.GetValues(typeof(T));
            return items.Select(x => new { Id = (int)Convert.ChangeType(x, TypeCode.Int32), Text = string.Join(",", x.GetValueText()) });
        }
        public static IEnumerable<object> getComplexEnumItems<T>() where T : Enum
        {
            var items = (T[])Enum.GetValues(typeof(T));
            return items.Select(x => new { Ids = x.GetValueId(), Texts = x.GetValueText() });
        }
        public static TOutput InitInhertedProperties<TInput, TOutput>(TInput baseClassInstance)
        {
            TOutput output = (TOutput)Activator.CreateInstance(typeof(TOutput));
            foreach (PropertyInfo propertyInfo in typeof(TInput).GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public))
            {
                object value = propertyInfo.GetValue(baseClassInstance);
                if (value != null) propertyInfo.SetValue(output, value);
            }
            return output;
        }
    }
}