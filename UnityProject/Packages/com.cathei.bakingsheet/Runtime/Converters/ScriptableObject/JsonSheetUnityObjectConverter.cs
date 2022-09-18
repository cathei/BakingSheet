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
            if (!(reader.Value is int referenceIndex))
                referenceIndex = -1;

            if (referenceIndex < 0)
                return null;

            if (referenceIndex >= _references.Count)
                throw new IndexOutOfRangeException();

            return _references[referenceIndex];
        }

        public override void WriteJson(
            JsonWriter writer, UnityEngine.Object value, JsonSerializer serializer)
        {
            int referenceIndex = -1;

            if (value != null)
            {
                referenceIndex = _references.IndexOf(value);

                if (referenceIndex < 0)
                {
                    referenceIndex = _references.Count;
                    _references.Add(value);
                }
            }

            writer.WriteValue(referenceIndex);
        }
    }
}
