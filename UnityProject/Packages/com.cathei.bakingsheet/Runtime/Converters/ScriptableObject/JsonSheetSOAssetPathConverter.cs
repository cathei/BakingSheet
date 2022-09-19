// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetSOAssetPathConverter : JsonConverter<IUnitySheetAssetPath>
    {
        public override IUnitySheetAssetPath ReadJson(
            JsonReader reader, Type objectType, IUnitySheetAssetPath existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (IUnitySheetAssetPath)Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, IUnitySheetAssetPath value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Asset);
        }
    }
}
