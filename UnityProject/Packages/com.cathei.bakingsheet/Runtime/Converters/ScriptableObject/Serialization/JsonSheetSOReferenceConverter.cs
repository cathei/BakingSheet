// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Unity
{
    public class JsonSheetSOReferenceConverter : JsonConverter<IUnitySheetReference>
    {
        public override IUnitySheetReference ReadJson(
            JsonReader reader, Type objectType, IUnitySheetReference existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (IUnitySheetReference)Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, IUnitySheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Asset);
        }
    }
}
