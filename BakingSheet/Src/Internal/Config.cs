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
        public const string Wildcard = "*";

        public static bool IsConvertable(Type type)
        {
            if (type.IsPrimitive || type.IsEnum)
                return true;

            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(TimeSpan))
                return true;

            if (typeof(ISheetReference).IsAssignableFrom(type))
                return true;

            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
        }

    }
}
