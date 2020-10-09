using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public abstract class SheetRow<TKey>
    {
        public TKey Id { get; internal set; }

        public virtual void ConvertFromRaw(SheetConvertingContext context, RawSheetRow row)
        {
            context.SetTag(context.Tag, Id);
            SheetUtility.ConvertFromRaw(context, this, row[0]);
        }

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
        [JsonIgnore]
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

    [JsonObject]
    public abstract class SheetRowArray<TKey, TElem> : SheetRow<TKey>, IEnumerable<TElem>
        where TElem : SheetRowElem, new()
    {
        [JsonProperty]
        protected List<TElem> Arr { get; private set; }

        [JsonIgnore]
        public int Count => Arr.Count;

        public TElem this[int idx] => Arr[idx];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<TElem> GetEnumerator() => Arr.GetEnumerator();

        public override void ConvertFromRaw(SheetConvertingContext context, RawSheetRow row)
        {
            base.ConvertFromRaw(context, row);

            Arr = new List<TElem>();

            var parentTag = context.Tag;

            foreach (var item in row)
            {
                context.SetTag(parentTag, Id, Arr.Count);

                var elem = new TElem();
                SheetUtility.ConvertFromRaw(context, elem, item);
                Arr.Add(elem);
            }
        }

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
