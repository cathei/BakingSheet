// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Unity
{
    internal class JsonSerializedUnityReference
    {
        [JsonProperty(SheetMetaType.PropertyName)]
        public string MetaType { get; set; }
        public int Value { get; set; }
    }

    public class JsonSheetUnityObjectConverter : JsonConverter<UnityEngine.Object>
    {
        private readonly List<UnityEngine.Object> _references;

        public JsonSheetUnityObjectConverter(List<UnityEngine.Object> references)
        {
            _references = references;
        }

        public override UnityEngine.Object ReadJson(
            JsonReader reader, Type objectType, UnityEngine.Object existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var serialized = serializer.Deserialize<JsonSerializedUnityReference>(reader);
            int referenceIndex = serialized?.Value ?? -1;

            if (referenceIndex < 0)
                return null;

            if (referenceIndex >= _references.Count)
                throw new IndexOutOfRangeException($"Reference index {referenceIndex} out of range {_references.Count}");

            return _references[referenceIndex];
        }

        public override void WriteJson(
            JsonWriter writer, UnityEngine.Object value, JsonSerializer serializer)
        {
            var serialized = new JsonSerializedUnityReference
            {
                MetaType = SheetMetaType.UnityObject,
                Value = -1
            };

            if (value != null)
            {
                serialized.Value = _references.IndexOf(value);

                if (serialized.Value < 0)
                {
                    serialized.Value = _references.Count;
                    _references.Add(value);
                }
            }

            serializer.Serialize(writer, serialized);
        }
    }
}
