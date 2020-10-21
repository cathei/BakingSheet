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
        public virtual Type TargetType => typeof(object);

        public abstract string Verify(Attribute attribute, object value);
    }

    public abstract class SheetVerifier<TAttr, TValue> : SheetVerifier
        where TAttr : SheetAssetAttribute
    {
        public sealed override Type TargetAttribute => typeof(TAttr);
        public sealed override Type TargetType => typeof(TValue);

        public sealed override string Verify(Attribute attribute, object value)
        {
            return Verify((TAttr)attribute, (TValue)value);
        }

        public abstract string Verify(TAttr attribute, TValue value);
    }
}
