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
                existingValue.FullPath = null;
                return existingValue;
            }

            existingValue.FullPath = $"{existingValue.Prefix}{path}{existingValue.Postfix}";
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetAssetPath value, JsonSerializer serializer)
        {
            writer.WriteValue(AssetPathValueConverter.ExtractPath(value));
        }
    }
}
