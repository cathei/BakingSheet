// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class DateTimeValueConverter : SheetValueConverter<DateTime>
    {
        protected override DateTime StringToValue(string value, ISheetFormatter format)
        {
            var local = DateTime.Parse(value, format.FormatProvider);
            return TimeZoneInfo.ConvertTimeToUtc(local, format.TimeZoneInfo);
        }

        protected override string ValueToString(DateTime value, ISheetFormatter format)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(value, format.TimeZoneInfo);
            return local.ToString(format.FormatProvider);
        }
    }
}
