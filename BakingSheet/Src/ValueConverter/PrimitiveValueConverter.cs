// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class PrimitiveValueConverter : ISheetValueConverter
    {
        public bool CanConvert(Type type)
        {
            return type.IsPrimitive || type.IsEnum ||
                   type == typeof(string) || type == typeof(decimal);
        }

        public object StringToValue(Type type, string value, ISheetFormatter format)
        {
            return Convert.ChangeType(value, type, format.FormatProvider);
        }

        public string ValueToString(Type type, object value, ISheetFormatter format)
        {
            return Convert.ToString(value, format.FormatProvider);
        }
    }
}
