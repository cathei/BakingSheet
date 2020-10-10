using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public interface ISheetReference
    {
        void Map(SheetConvertingContext context);

        void ReadJson(JsonReader reader, JsonSerializer serializer);
        void WriteJson(JsonWriter writer, JsonSerializer serializer);
    }

    public interface ISheetReference<T> : ISheetReference
    {
        T Id { get; }
    }

    public partial class Sheet<TKey, TValue>
    {
        public struct Reference : ISheetReference<TKey>
        {
            public TKey Id { get; private set; }
            public TValue Ref { get; private set; }

            public Reference(TKey id)
            {
                Id = id;
                Ref = null;
            }

            void ISheetReference.Map(SheetConvertingContext context)
            {
                var sheet = context.Container.AllSheets
                    .FirstOrDefault(x => x.GetType().IsSubclassOf(typeof(Sheet<TKey, TValue>)));
                
                if (sheet != null)
                {
                    Ref = (sheet as Sheet<TKey, TValue>)[Id];
                }

                if (Id != null && Ref == null)
                {
                    context.Logger.LogError($"[{context.Tag}] Failed to find reference \"{Id}\" on {sheet.Name}");
                }
            }

            void ISheetReference.ReadJson(JsonReader reader, JsonSerializer serializer)
            {
                Id = (TKey)serializer.Deserialize(reader, typeof(TKey));
            }

            void ISheetReference.WriteJson(JsonWriter writer, JsonSerializer serializer)
            {
                serializer.Serialize(writer, Id);
            }

            public static implicit operator TKey(Reference origin)
            {
                return origin.Id;
            }

            public override bool Equals(object obj)
            {
                return obj is Reference && this == (Reference)obj;
            }

            public override int GetHashCode()
            {
                return Id == null ? 0 : Id.GetHashCode();
            }

            public override string ToString()
            {
                return Id == null ? "(null)" : Id.ToString();
            }

            public static bool operator ==(Reference x, Reference y)
            {
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                    return ReferenceEquals(x, null) && ReferenceEquals(y, null);

                if (x.Id == null || y.Id == null)
                    return x.Id == null && y.Id == null;

                return x.GetType() == y.GetType() && x.Id.Equals(y.Id);
            }

            public static bool operator !=(Reference x, Reference y)
            {
                return !(x == y);
            }
        }
    }
}
