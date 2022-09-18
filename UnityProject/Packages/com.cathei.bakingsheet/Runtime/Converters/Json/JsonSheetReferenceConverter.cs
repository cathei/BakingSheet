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
    public class JsonSheetReferenceConverter : JsonConverter<ISheetReference>
    {
        public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (existingValue == null)
                existingValue = (ISheetReference)Activator.CreateInstance(objectType);

            existingValue.Id = serializer.Deserialize(reader, existingValue.IdType);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Id);
        }
    }
}
