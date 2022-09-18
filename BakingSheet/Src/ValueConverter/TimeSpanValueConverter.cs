// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class TimeSpanValueConverter : SheetValueConverter<TimeSpan>
    {
        protected override TimeSpan StringToValue(string value, SheetValueConvertingContext context)
        {
            return TimeSpan.Parse(value, context.FormatProvider);
        }

        protected override string ValueToString(TimeSpan value, SheetValueConvertingContext context)
        {
            return value.ToString(null, context.FormatProvider);
        }
    }
}
