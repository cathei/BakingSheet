// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class NullableValueConverter : ISheetValueConverter
    {
        public bool CanConvert(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public object StringToValue(Type type, string value, ISheetFormatter format)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return format.StringToValue(underlyingType, value);
        }

        public string ValueToString(Type type, object value, ISheetFormatter format)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return format.ValueToString(underlyingType, value);
        }
    }
}
