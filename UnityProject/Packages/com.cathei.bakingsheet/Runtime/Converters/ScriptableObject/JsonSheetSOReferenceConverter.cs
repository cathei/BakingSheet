// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetSOReferenceConverter : JsonConverter<ISheetReference>
    {
        public override ISheetReference ReadJson(
            JsonReader reader, Type objectType, ISheetReference existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (ISheetReference)Activator.CreateInstance(objectType);
            existingValue.SO = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, ISheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.SO);
        }
    }
}
