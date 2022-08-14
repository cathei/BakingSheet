using System;
using System.Collections;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public abstract class SheetRow<TKey> : ISheetRow<TKey>
    {
        [Preserve]
        public TKey Id { get; set; }

        object ISheetRow.Id => Id;

        public virtual void PostLoad(SheetConvertingContext context) { }
        public virtual void VerifyAssets(SheetConvertingContext context) { }
    }

    public abstract class SheetRowElem : ISheetRowElem
    {
        [NonSerialized]
        public int Index { get; internal set; }

        public virtual void PostLoad(SheetConvertingContext context) { }
        public virtual void VerifyAssets(SheetConvertingContext context) { }
    }

    public abstract class SheetRowArray<TKey, TElem> : SheetRow<TKey>, ISheetRowArray<TElem>
        where TElem : SheetRowElem, new()
    {
        [Preserve]
        // setter is necessary for reflection
        public VerticalList<TElem> Arr { get; private set; } = new VerticalList<TElem>();

        IReadOnlyList<object> ISheetRowArray.Arr => Arr;
        public Type ElemType => typeof(TElem);

        IReadOnlyList<TElem> ISheetRowArray<TElem>.Arr => Arr;
        public int Count => Arr.Count;
        public TElem this[int idx] => Arr[idx];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<TElem> GetEnumerator() => Arr.GetEnumerator();

        public override void PostLoad(SheetConvertingContext context)
        {
            base.PostLoad(context);

            for (int idx = 0; idx < Arr.Count; ++idx)
            {
                using (context.Logger.BeginScope(idx))
                {
                    Arr[idx].Index = idx;
                    Arr[idx].PostLoad(context);
                }
            }
        }

        public override void VerifyAssets(SheetConvertingContext context)
        {
            base.VerifyAssets(context);

            for (int idx = 0; idx < Arr.Count; ++idx)
            {
                using (context.Logger.BeginScope(idx))
                {
                    Arr[idx].VerifyAssets(context);
                }
            }
        }
    }

    // Convenient shorthand
    public abstract class SheetRow : SheetRow<string> {}

    // Convenient shorthand
    public abstract class SheetRowArray<TElem> : SheetRowArray<string, TElem>
        where TElem : SheetRowElem, new() {}
}
