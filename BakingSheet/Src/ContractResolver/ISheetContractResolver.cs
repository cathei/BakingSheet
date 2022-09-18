// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Reflection;

namespace Cathei.BakingSheet
{
    public interface ISheetContractResolver
    {
        ISheetValueConverter GetValueConverter(Type type);
        ISheetValueConverter GetValueConverter(PropertyInfo propertyInfo);
    }
}
