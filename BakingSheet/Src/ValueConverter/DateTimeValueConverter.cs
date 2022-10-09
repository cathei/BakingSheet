// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;

namespace Cathei.BakingSheet.Internal
{
    public class DateTimeValueConverter : SheetValueConverter<DateTime>
    {
        protected override DateTime StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            var local = DateTime.Parse(value, context.FormatProvider);
            return TimeZoneInfo.ConvertTimeToUtc(local, context.TimeZoneInfo);
        }

        protected override string ValueToString(Type type, DateTime value, SheetValueConvertingContext context)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(value, context.TimeZoneInfo);
            return local.ToString(context.FormatProvider);
        }
    }
}
