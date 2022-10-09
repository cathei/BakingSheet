// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Unity;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Unity
{
    internal class JsonSerializedAssetPath
    {
        [JsonProperty(SheetMetaType.PropertyName)]
        public string MetaType { get; set; }

        [JsonProperty]
        public string RawValue { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public UnityEngine.Object Asset { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FullPath { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SubAssetName { get; set; }
    }

    public class JsonSheetSOAssetPathConverter : JsonConverter<IUnitySheetAssetPath>
    {
        public override IUnitySheetAssetPath ReadJson(
            JsonReader reader, Type objectType, IUnitySheetAssetPath existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var serialized = serializer.Deserialize<JsonSerializedAssetPath>(reader);
            var value = (IUnitySheetAssetPath)Activator.CreateInstance(objectType, serialized?.RawValue);

            if (value is IUnitySheetDirectAssetPath directPath)
                directPath.Asset = serialized?.Asset;

            return value;
        }

        public override void WriteJson(
            JsonWriter writer, IUnitySheetAssetPath value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var serialized = new JsonSerializedAssetPath
            {
                MetaType = value.MetaType,
                RawValue = value.RawValue,
            };

            if (value is IUnitySheetDirectAssetPath directPath)
            {
                serialized.Asset = directPath.Asset;
            }
            else
            {
                serialized.FullPath = value.FullPath;
                serialized.SubAssetName = value.SubAssetName;
            }

            serializer.Serialize(writer, serialized);
        }
    }
}
