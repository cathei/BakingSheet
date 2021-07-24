using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    internal class SheetReferenceConverter : JsonConverter<ISheetReference>
    {
        public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            existingValue.Id = serializer.Deserialize(reader, existingValue.IdType);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Id);
        }
    }
}
