using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cathei.BakingSheet
{
    // Used for reflection
    public interface ISheet : IList
    {
        string Name { get; set; }
        Type RowType { get; }

        void PostLoad(SheetConvertingContext context);
        void VerifyAssets(SheetConvertingContext context);
    }

    public abstract partial class Sheet<TKey, TValue> : KeyedCollection<TKey, TValue>, ISheet
        where TValue : SheetRow<TKey>, new()
    {
        public string Name { get; set; }
        public Type RowType => typeof(TValue);

        public ICollection<TKey> Keys => Dictionary.Keys;
        public ICollection<TValue> Values => Dictionary.Values;

        public new TValue this[TKey id]
        {
            get
            {
                if (!Contains(id))
                    return default(TValue);
                return base[id];
            }
        }

        protected override TKey GetKeyForItem(TValue item)
        {
            return item.Id;
        }

        public virtual void PostLoad(SheetConvertingContext context)
        {
            foreach (var row in Values)
            {
                context.SetTag(Name);
                row.PostLoad(context);
            }
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            foreach (var row in Values)
            {
                context.SetTag(Name);
                row.VerifyAssets(context);
            }
        }
    }

    // Convenient shorthand
    public abstract class Sheet<T> : Sheet<string, T>
        where T : SheetRow<string>, new() {}
}
