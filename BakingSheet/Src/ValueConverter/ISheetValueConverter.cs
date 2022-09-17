// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public interface ISheetValueConverter
    {
        bool CanConvert(Type type);
        object StringToValue(Type type, string value, ISheetFormatter format);
        string ValueToString(Type type, object value, ISheetFormatter format);
    }
}
