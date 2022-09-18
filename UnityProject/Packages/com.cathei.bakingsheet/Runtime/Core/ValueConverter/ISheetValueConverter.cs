// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public readonly struct SheetValueConvertingContext
    {
        private readonly ISheetFormatter _format;
        private readonly ISheetContractResolver _resolver;

        public TimeZoneInfo TimeZoneInfo => _format.TimeZoneInfo;
        public IFormatProvider FormatProvider => _format.FormatProvider;

        public SheetValueConvertingContext(ISheetFormatter format, ISheetContractResolver resolver)
        {
            _format = format;
            _resolver = resolver;
        }

        private ISheetValueConverter GetValueConverter(Type type)
        {
            return _resolver.GetValueConverter(type);
        }

        public string ValueToString(Type type, object value)
        {
            if (value == null)
                return null;

            var converter = GetValueConverter(type);
            return converter.ValueToString(type, value, this);
        }

        public object StringToValue(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var converter = GetValueConverter(type);
            return converter.StringToValue(type, value, this);
        }
    }

    public interface ISheetValueConverter
    {
        bool CanConvert(Type type);
        object StringToValue(Type type, string value, SheetValueConvertingContext context);
        string ValueToString(Type type, object value, SheetValueConvertingContext context);
    }
}
