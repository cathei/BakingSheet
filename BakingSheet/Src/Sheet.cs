using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cathei.BakingSheet
{
    // Used for reflection
    public interface ISheet : IEnumerable
    {
        string Name { get; }
        Type RowType { get; }

        int Count { get; }
        bool Contains(object key);
        void Add(object value);

        void PostLoad(SheetConvertingContext context);
        void VerifyAssets(SheetConvertingContext context);
    }

    public abstract partial class Sheet<TKey, TValue> : KeyedCollection<TKey, TValue>, ISheet
        where TValue : SheetRow<TKey>, new()
    {
        public string Name { get; private set; }

        public Type RowType => typeof(TValue);

        public new TValue this[TKey id]
        {
            get
            {
                if (!Contains(id))
                    return default(TValue);
                return base[id];
            }
        }

        bool ISheet.Contains(object key) => Contains((TKey)key);
        void ISheet.Add(object value) => Add((TValue)value);

        protected override TKey GetKeyForItem(TValue item)
        {
            return item.Id;
        }

        public virtual void PostLoad(SheetConvertingContext context)
        {
            Name = context.Tag;

            foreach (var row in Items)
            {
                row.PostLoad(context);
            }
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            foreach (var row in Items)
            {
                row.VerifyAssets(context);
            }
        }
    }
    
    // Convenient shorthand
    public abstract class Sheet<T> : Sheet<string, T>
        where T : SheetRow<string>, new() {}
}
