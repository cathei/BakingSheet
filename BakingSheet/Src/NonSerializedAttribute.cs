using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class NonSerializedAttribute : Attribute
    {

    }
}
