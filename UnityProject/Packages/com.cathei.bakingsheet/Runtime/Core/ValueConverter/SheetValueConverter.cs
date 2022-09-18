// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public abstract class SheetValueConverter<T> : ISheetValueConverter
    {
        bool ISheetValueConverter.CanConvert(Type type)
        {
            return type == typeof(T);
        }

        object ISheetValueConverter.StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            return StringToValue(value, context);
        }

        string ISheetValueConverter.ValueToString(Type type, object value, SheetValueConvertingContext context)
        {
            return ValueToString((T)value, context);
        }

        protected abstract T StringToValue(string value, SheetValueConvertingContext context);
        protected abstract string ValueToString(T value, SheetValueConvertingContext context);

    }
}
