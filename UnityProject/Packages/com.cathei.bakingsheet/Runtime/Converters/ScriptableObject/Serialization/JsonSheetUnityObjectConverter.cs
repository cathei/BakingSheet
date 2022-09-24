// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Object = UnityEngine.Object;

namespace Cathei.BakingSheet
{
    internal class JsonSheetUnityReference
    {
        [JsonProperty("$asset")]
        public int Value { get; set; }
    }

    public class JsonSheetUnityObjectConverter : JsonConverter<UnityEngine.Object>
    {
        private readonly List<Object> _references;

        public JsonSheetUnityObjectConverter(List<UnityEngine.Object> references)
        {
            _references = references;
        }

        public override UnityEngine.Object ReadJson(
            JsonReader reader, Type objectType, UnityEngine.Object existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var reference = serializer.Deserialize<JsonSheetUnityReference>(reader);

            if (reference == null || reference.Value < 0)
                return null;

            if (reference.Value >= _references.Count)
                throw new IndexOutOfRangeException($"Reference index {reference.Value} out of range {_references.Count}");

            return _references[reference.Value];
        }

        public override void WriteJson(
            JsonWriter writer, UnityEngine.Object value, JsonSerializer serializer)
        {
            var reference = new JsonSheetUnityReference
            {
                Value = -1
            };

            if (value != null)
            {
                reference.Value = _references.IndexOf(value);

                if (reference.Value < 0)
                {
                    reference.Value = _references.Count;
                    _references.Add(value);
                }
            }

            serializer.Serialize(writer, reference);
        }
    }
}
