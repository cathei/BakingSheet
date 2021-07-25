using System;
using System.Collections;
using System.Collections.Generic;

namespace Cathei.BakingSheet
{
    public interface ISheetRow
    {
        object Id { get; }
    }

    public interface ISheetRowArray
    {
        IList Arr { get; }
        Type ElemType { get; }
    }
    
    public abstract class SheetRow<TKey> : ISheetRow
    {
        public TKey Id { get; protected internal set; }

        object ISheetRow.Id => Id;

        public virtual void PostLoad(SheetConvertingContext context)
        {
            context.SetTag(context.Tag, Id);
            SheetUtility.MapReferences(context, this);
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            context.SetTag(context.Tag, Id);
            SheetUtility.VerifyAssets(context, this);
        }
    }

    public abstract class SheetRowElem
    {
        [NonSerialized]
        public int Index { get; internal set; } 

        public virtual void PostLoad(SheetConvertingContext context)
        {
            context.SetTag(context.Tag, Index);
            SheetUtility.MapReferences(context, this);
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            context.SetTag(context.Tag, Index);
            SheetUtility.VerifyAssets(context, this);
        }
    }

    public abstract class SheetRowArray<TKey, TElem> : SheetRow<TKey>, IEnumerable<TElem>, ISheetRowArray
        where TElem : SheetRowElem, new()
    {
        public List<TElem> Arr { get; private set; } = new List<TElem>();

        IList ISheetRowArray.Arr => Arr;
        public Type ElemType => typeof(TElem);

        public int Count => Arr.Count;
        public TElem this[int idx] => Arr[idx];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<TElem> GetEnumerator() => Arr.GetEnumerator();

        public override void PostLoad(SheetConvertingContext context)
        {
            base.PostLoad(context);

            var parentTag = context.Tag;

            for (int idx = 0; idx < Arr.Count; ++idx)
            {
                context.SetTag(parentTag, Id);

                Arr[idx].Index = idx;
                Arr[idx].PostLoad(context);
            }
        }

        public override void VerifyAssets(SheetConvertingContext context)
        {
            base.VerifyAssets(context);

            var parentTag = context.Tag;

            for (int idx = 0; idx < Arr.Count; ++idx)
            {
                context.SetTag(parentTag, Id);
                Arr[idx].VerifyAssets(context);
            }
        }
    }

    // Convenient shorthand
    public abstract class SheetRow : SheetRow<string> {}

    // Convenient shorthand
    public abstract class SheetRowArray<TElem> : SheetRowArray<string, TElem>
        where TElem : SheetRowElem, new() {}
}
