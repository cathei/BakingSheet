using System;
using System.Collections;
using System.Collections.Generic;

namespace Cathei.BakingSheet
{
    // Used for reflection
    public abstract class Sheet
    {
        public string Name { get; internal set; }
        public abstract Type RowType { get; }

        public abstract void PostLoad(SheetConvertingContext context);
        public abstract void VerifyAssets(SheetConvertingContext context);
    }

    public abstract partial class Sheet<TKey, TValue> : Sheet, IDictionary<TKey, TValue>, IDictionary
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

        public override Type RowType => typeof(TValue);

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

        object IDictionary.this[object key]
        {
            get => ((IDictionary)Data)[key];
            set => ((IDictionary)Data)[key] = value;
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => Data[key];
            set => Data[key] = value;
        }

        bool IDictionary.IsFixedSize => ((IDictionary)Data).IsFixedSize;
        bool IDictionary.IsReadOnly => ((IDictionary)Data).IsReadOnly;

        ICollection IDictionary.Keys => Data.Keys;
        ICollection IDictionary.Values => Data.Values;

        void IDictionary.Add(object key, object value) => Data.Add((TKey)key, (TValue)value);
        void IDictionary.Clear() => Data.Clear();
        bool IDictionary.Contains(object key) => ((IDictionary)Data).Contains(key);
        void IDictionary.Remove(object key) => ((IDictionary)Data).Remove(key);

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Data.Add(key, value);
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => Data.ContainsKey(key);
        bool IDictionary<TKey, TValue>.Remove(TKey key) => Data.Remove(key);
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => Data.TryGetValue(key, out value);

        bool ICollection.IsSynchronized => ((ICollection)Data).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)Data).SyncRoot;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Add(item);
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ((IDictionary<TKey, TValue>)Data).Clear();
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Contains(item);

        void ICollection.CopyTo(Array array, int index) => ((IDictionary)Data).CopyTo(array, index);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Data).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Data).Remove(item);

        IDictionaryEnumerator IDictionary.GetEnumerator() => Data.GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => Data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();

        #endregion
    }

    // Convenient shorthand
    public abstract class Sheet<T> : Sheet<string, T>
        where T : SheetRow<string>, new() {}
}
