// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Represents a Row of a Sheet.
    /// </summary>
    /// <typeparam name="TKey">Type of Id column.</typeparam>
    public abstract class SheetRow<TKey> : ISheetRow<TKey>
    {
        [Preserve] public TKey Id { get; set; }
        [NonSerialized] public int Index { get; internal set; }

        object ISheetRow.Id => Id;

        public virtual void PostLoad(SheetConvertingContext context) { }
        public virtual void VerifyAssets(SheetConvertingContext context) { }
    }

    /// <summary>
    /// Represents an Element of Row Array.
    /// </summary>
    public abstract class SheetRowElem : ISheetRowElem
    {
        [NonSerialized] public int Index { get; internal set; }

        public virtual void PostLoad(SheetConvertingContext context)
        {
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
        }
    }

    /// <summary>
    /// Represents a Row of a Sheet, that vertically having multiple Elements.
    /// </summary>
    /// <typeparam name="TKey">Type of Id column.</typeparam>
    /// <typeparam name="TElem">Type of Element.</typeparam>
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
        IEnumerator<TElem> IEnumerable<TElem>.GetEnumerator() => GetEnumerator();
        public Enumerator GetEnumerator() => new Enumerator(Arr.GetEnumerator());

        public override void PostLoad(SheetConvertingContext context)
        {
            base.PostLoad(context);

            int index = -1;

            foreach (var elem in Arr)
            {
                using (context.Logger.BeginScope(++index))
                {
                    elem.Index = index;
                    elem.PostLoad(context);
                }
            }
        }

        public override void VerifyAssets(SheetConvertingContext context)
        {
            base.VerifyAssets(context);

            int index = -1;

            foreach (var elem in Arr)
            {
                using (context.Logger.BeginScope(++index))
                {
                    elem.VerifyAssets(context);
                }
            }
        }

        /// <summary>
        /// Struct enumerator for SheetRowArray.
        /// </summary>
        public struct Enumerator : IEnumerator<TElem>
        {
            private List<TElem>.Enumerator _enumerator;

            public Enumerator(List<TElem>.Enumerator enumerator)
            {
                _enumerator = enumerator;
            }

            public bool MoveNext() => _enumerator.MoveNext();
            public TElem Current => _enumerator.Current;

            void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
            object IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
        }
    }

    /// <summary>
    /// Represent a Row of a Sheet with string Id.
    /// For other type of Id, use generic version.
    /// </summary>
    public abstract class SheetRow : SheetRow<string> {}

    /// <summary>
    /// Represents a Row of a Sheet, that vertically having multiple Elements, with string Id.
    /// For other type of Id, use generic version.
    /// </summary>
    public abstract class SheetRowArray<TElem> : SheetRowArray<string, TElem>
        where TElem : SheetRowElem, new() {}
}
