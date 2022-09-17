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

        object ISheetValueConverter.StringToValue(Type type, string value, ISheetFormatter format)
        {
            return StringToValue(value, format);
        }

        string ISheetValueConverter.ValueToString(Type type, object value, ISheetFormatter format)
        {
            return ValueToString((T)value, format);
        }

        protected abstract T StringToValue(string value, ISheetFormatter format);
        protected abstract string ValueToString(T value, ISheetFormatter format);

    }
}
