// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetSOAssetPathConverter : JsonConverter<IUnitySheetDirectAssetPath>
    {
        public override IUnitySheetDirectAssetPath ReadJson(
            JsonReader reader, Type objectType, IUnitySheetDirectAssetPath existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (IUnitySheetDirectAssetPath)Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, IUnitySheetDirectAssetPath value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Asset);
        }
    }
}
