using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cathei.BakingSheet
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class SheetAssetAttribute : Attribute
    {

    }

    public abstract class SheetVerifier
    {
        public virtual Type TargetAttribute => typeof(SheetAssetAttribute);

        public abstract string Verify(Attribute attribute);
    }

    public abstract class SheetVerifier<T> : SheetVerifier
        where T : SheetAssetAttribute
    {
        public sealed override Type TargetAttribute => typeof(T);

        public sealed override string Verify(Attribute attribute)
        {
            return Verify(attribute as T);
        }

        public abstract string Verify(T attribute);
    }
}
