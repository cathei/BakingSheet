// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Unity;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Unity
{
    internal struct JsonSerializedDirectAssetPath
    {
        [JsonProperty("$type")]
        public string MetaType { get; set; }
        public string RawValue { get; set; }
        public UnityEngine.Object Asset { get; set; }
    }

    public class JsonSheetSOAssetPathConverter : JsonConverter<IUnitySheetAssetPath>
    {
        public override IUnitySheetAssetPath ReadJson(
            JsonReader reader, Type objectType, IUnitySheetAssetPath existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var serialized = serializer.Deserialize<JsonSerializedDirectAssetPath?>(reader);
            if (serialized == null)
                return null;

            existingValue ??= (IUnitySheetAssetPath)Activator.CreateInstance(objectType);
            existingValue.RawValue = serialized?.RawValue;

            if (existingValue is IUnitySheetDirectAssetPath directPath)
                directPath.Asset = serialized?.Asset;

            return existingValue;
        }

        public override void WriteJson(
            JsonWriter writer, IUnitySheetAssetPath value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var serialized = new JsonSerializedDirectAssetPath
            {
                MetaType = value.MetaType,
                RawValue = value.RawValue,
            };

            if (value is IUnitySheetDirectAssetPath directPath)
                serialized.Asset = directPath.Asset;

            serializer.Serialize(writer, serialized);
        }
    }
}
