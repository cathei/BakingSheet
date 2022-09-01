using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public static class Config
    {
        public const string Delimiter = ":";
        public const string Comment = "$";

        public static bool IsConvertable(Type type)
        {
            // is it numeric type?
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal))
                return true;

            // is it string or date type?
            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(TimeSpan))
                return true;

            // is it sheet reference?
            if (typeof(ISheetReference).IsAssignableFrom(type))
                return true;

            // is it nullable value type?
            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
        }

    }
}
