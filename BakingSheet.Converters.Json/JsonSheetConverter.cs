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
    public class JsonSheetConverter : ISheetConverter
    {
        public virtual string Extension => "json";

        private string _loadPath;
        private IFileSystem _fileSystem;

        public JsonSheetConverter(string path, IFileSystem fileSystem = null)
        {
            _loadPath = path;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public virtual JsonSerializerSettings GetSettings(ILogger logError)
        {
            var settings = new JsonSerializerSettings();

            settings.Error = (sender, err) =>
            {
                if (err.ErrorContext.Member?.ToString() == nameof(ISheetRow.Id) &&
                    err.ErrorContext.OriginalObject is ISheetRow &&
                    !(err.CurrentObject is ISheet))
                {
                    // if Id has error, the error must be handled on the sheet level
                    return;
                }

                using (logError.BeginScope(err.ErrorContext.Path))
                    logError.LogError(err.ErrorContext.Error, err.ErrorContext.Error.Message);

                err.ErrorContext.Handled = true;
            };

            settings.ContractResolver = new JsonSheetContractResolver();

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new SheetReferenceConverter());

            return settings;
        }

        protected virtual string Serialize(object obj, Type type, ILogger logger)
        {
            return JsonConvert.SerializeObject(obj, type, GetSettings(logger));
        }

        protected virtual object Deserialize(string json, Type type, ILogger logger)
        {
            return JsonConvert.DeserializeObject(json, type, GetSettings(logger));
        }

        public async Task<bool> Import(SheetConvertingContext context)
        {
            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var path = Path.Combine(_loadPath, $"{prop.Name}.{Extension}");

                    if (!_fileSystem.Exists(path))
                        continue;

                    string data;

                    using (var stream = _fileSystem.OpenRead(path))
                    using (var reader = new StreamReader(stream))
                        data = await reader.ReadToEndAsync();

                    var sheet = Deserialize(data, prop.PropertyType, context.Logger) as ISheet;
                    prop.SetValue(context.Container, sheet);
                }
            }

            return true;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container);
                    var data = Serialize(sheet, prop.PropertyType, context.Logger);

                    var path = Path.Combine(_loadPath, $"{prop.Name}.{Extension}");

                    using (var stream = _fileSystem.OpenWrite(path))
                    using (var writer = new StreamWriter(stream))
                        await writer.WriteAsync(data);
                }
            }

            return true;
        }

        internal class SheetReferenceConverter : JsonConverter<ISheetReference>
        {
            public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (existingValue == null)
                    existingValue = Activator.CreateInstance(objectType) as ISheetReference;

                existingValue.Id = serializer.Deserialize(reader, existingValue.IdType);
                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.Id);
            }
        }
    }
}
