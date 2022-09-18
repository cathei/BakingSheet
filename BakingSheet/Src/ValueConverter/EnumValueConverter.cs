// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class EnumValueConverter : ISheetValueConverter
    {
        public bool CanConvert(Type type)
        {
            return type.IsEnum;
        }

        public object StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            return Enum.Parse(type, value, true);
        }

        public string ValueToString(Type type, object value, SheetValueConvertingContext context)
        {
            return value.ToString();
        }
    }
}
