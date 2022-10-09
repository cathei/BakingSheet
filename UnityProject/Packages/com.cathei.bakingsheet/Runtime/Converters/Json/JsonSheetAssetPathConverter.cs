// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetAssetPathConverter : JsonConverter<ISheetAssetPath>
    {
        public override ISheetAssetPath ReadJson(JsonReader reader, Type objectType, ISheetAssetPath existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string path = (string)reader.Value;
            return (ISheetAssetPath)Activator.CreateInstance(objectType, path);
        }

        public override void WriteJson(JsonWriter writer, ISheetAssetPath value, JsonSerializer serializer)
        {
            if (!value.IsValid())
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.RawValue);
        }
    }
}
