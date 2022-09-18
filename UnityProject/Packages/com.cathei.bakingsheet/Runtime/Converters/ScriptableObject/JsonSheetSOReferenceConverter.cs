// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetSOReferenceConverter : JsonConverter<ISheetScriptableObjectReference>
    {
        public override ISheetScriptableObjectReference ReadJson(
            JsonReader reader, Type objectType, ISheetScriptableObjectReference existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (ISheetScriptableObjectReference)Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, ISheetScriptableObjectReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Asset);
        }
    }
}
