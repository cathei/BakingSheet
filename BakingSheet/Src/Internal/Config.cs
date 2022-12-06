// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        /// <summary>
        /// Iterate properties with both getter and setter.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetEligibleProperties(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance |
                                              BindingFlags.DeclaredOnly;

            while (type != null)
            {
                var properties = type.GetProperties(bindingFlags);

                foreach (var property in properties)
                {
                    if (property.IsDefined(typeof(NonSerializedAttribute)))
                        continue;

                    if (property.GetMethod != null && property.SetMethod != null)
                        yield return property;
                }

                type = type.BaseType;
            }
        }

        public static PropertyInfo GetRowArrayProperty(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SheetRowArray<,>))
                {
                    return type.GetProperty(nameof(ISheetRowArray.Arr));
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
