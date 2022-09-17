// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public class PrimitiveValueConverter : ISheetValueConverter
    {
        public bool CanConvert(Type type)
        {

        }

        public object StringToValue(Type type, string value, ISheetFormatter format)
        {
        }

        public string ValueToString(Type type, object value, ISheetFormatter format)
        {
        }
    }
}
