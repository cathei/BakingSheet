// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;

namespace Cathei.BakingSheet.Internal
{
    public class NullableValueConverter : ISheetValueConverter
    {
        public bool CanConvert(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public object StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var underlyingType = Nullable.GetUnderlyingType(type);
            return context.StringToValue(underlyingType, value);
        }

        public string ValueToString(Type type, object value, SheetValueConvertingContext context)
        {
            if (value == null)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(type);
            return context.ValueToString(underlyingType, value);
        }
    }
}
