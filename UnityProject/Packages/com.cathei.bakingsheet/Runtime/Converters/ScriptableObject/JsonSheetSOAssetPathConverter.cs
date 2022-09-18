// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetSOAssetPathConverter : JsonConverter<ISheetAssetPath>
    {
        public override ISheetAssetPath ReadJson(
            JsonReader reader, Type objectType, ISheetAssetPath existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue ??= (ISheetAssetPath)Activator.CreateInstance(objectType);
            existingValue.Asset = serializer.Deserialize<SheetRowScriptableObject>(reader);
            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, ISheetAssetPath value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Asset);
        }
    }
}
