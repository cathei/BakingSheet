// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class JsonSheetAssetPathConverter : JsonConverter<ISheetAssetPath>
    {
        public override ISheetAssetPath ReadJson(JsonReader reader, Type objectType, ISheetAssetPath existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (existingValue == null)
                existingValue = (ISheetAssetPath)Activator.CreateInstance(objectType);

            string path = (string)reader.Value;

            if (string.IsNullOrEmpty(path))
            {
                existingValue.RelativePath = null;
                return existingValue;
            }

            existingValue.RelativePath = path;
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetAssetPath value, JsonSerializer serializer)
        {
            if (!value.IsValid())
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value.RelativePath);
        }
    }
}
