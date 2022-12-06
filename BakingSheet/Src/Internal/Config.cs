// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;

namespace Cathei.BakingSheet.Internal
{
    public static class Config
    {
        public const string Comment = "$";
        public const string IndexDelimiter = ":";
        public const string SheetNameDelimiter = ".";

        // TODO: in .net standard 2.1 this is not needed
        public static readonly string[] IndexDelimiterArray = { IndexDelimiter };

        /// <summary>
        /// Split SheetName.SubName format.
        /// </summary>
        public static (string name, string subName) ParseSheetName(string name)
        {
            int idx = name.IndexOf(SheetNameDelimiter, StringComparison.Ordinal);

            if (idx == -1)
                return (name, null);

            return (name.Substring(0, idx), name.Substring(idx + 1));
        }
    }
}
