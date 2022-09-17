// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
    public class SheetValueConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public SheetValueConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }
    }
}
