// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetReferenceConverter : JsonConverter<ISheetReference>
    {
        public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (existingValue == null)
                existingValue = (ISheetReference)Activator.CreateInstance(objectType);

            existingValue.Id = serializer.Deserialize(reader, existingValue.IdType);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Id);
        }
    }
}
