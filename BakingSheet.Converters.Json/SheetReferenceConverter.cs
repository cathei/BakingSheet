using System;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    internal class SheetReferenceConverter : JsonConverter<ISheetReference>
    {
        public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue.Id = serializer.Deserialize(reader, existingValue.IdType);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Id);
        }
    }
}
