// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Reflection;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Interface for contract resolver that determines and caches value converter.
    /// </summary>
    public interface ISheetContractResolver
    {
        ISheetValueConverter GetValueConverter(Type type);
        ISheetValueConverter GetValueConverter(PropertyInfo propertyInfo);
    }
}
