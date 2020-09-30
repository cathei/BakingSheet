using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    // Used for reflection
    public abstract class Sheet
    {
        internal string Name { get; set; }

        internal abstract void ConvertFromRaw(RawSheet gsheet, SheetConvertingContext context);
        public abstract void PostLoad(SheetConvertingContext context);
        public abstract void VerifyAssets(SheetConvertingContext context);
    }

    public abstract partial class Sheet<TKey, TValue> : Sheet, IDictionary<TKey, TValue>
        where TValue : SheetRow<TKey>, new()
    {
        private Dictionary<TKey, TValue> Data { get; } = new Dictionary<TKey, TValue>();

        public ICollection<TKey> Keys => Data.Keys;
        public ICollection<TValue> Values => Data.Values;

        public int Count => Data.Count;

        public TValue Find(TKey id)
        {
            if (id != null && Data.ContainsKey(id))
                return Data[id];
            return default(TValue);
        }

        public TValue this[TKey id] => Find(id);

        internal sealed override void ConvertFromRaw(RawSheet rawSheet, SheetConvertingContext context)
        {
            foreach (var rowData in rawSheet.Rows)
            {
                try
                {
                    context.SetTag(Name);

                    var row = new TValue();
                    row.ConvertFromRaw(context, rowData);
                    Data.Add(row.Id, row);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(ex, ex.Message);
                }
            }
        }

        public override void PostLoad(SheetConvertingContext context)
        {
            foreach (var row in Values)
            {
                context.SetTag(Name);
                row.PostLoad(context);
            }
        }

        public override void VerifyAssets(SheetConvertingContext context)
        {
            foreach (var row in Values)
            {
                context.SetTag(Name);
                row.VerifyAssets(context);
            }
        }

        #region IDictionary interface

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => Data[key];
            set => Data[key] = value;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Data.Add(key, value);
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => Data.ContainsKey(key);
        bool IDictionary<TKey, TValue>.Remove(TKey key) => Data.Remove(key);
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => Data.TryGetValue(key, out value);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Add(item);
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ((IDictionary<TKey, TValue>)Data).Clear();
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Data).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Remove(item);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();

        #endregion
    }

    // Convenient shorthand
    public abstract class Sheet<T> : Sheet<string, T>
        where T : SheetRow<string>, new() {}
}
