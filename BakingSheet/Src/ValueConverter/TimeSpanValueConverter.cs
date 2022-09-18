// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class TimeSpanValueConverter : SheetValueConverter<TimeSpan>
    {
        protected override TimeSpan StringToValue(string value, ISheetFormatter format)
        {
            return TimeSpan.Parse(value, format.FormatProvider);
        }

        protected override string ValueToString(TimeSpan value, ISheetFormatter format)
        {
            return value.ToString(null, format.FormatProvider);
        }
    }
}
