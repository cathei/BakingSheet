using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cathei.BakingSheet
{
    public class JsonSheetConverter : ISheetImporter, ISheetExporter
    {
        public virtual string Extension => "json";

        private string BasePath { get; }

        public JsonSheetConverter(string path)
        {
            BasePath = path;
        }

        public virtual JsonSerializerSettings GetSettings(ILogger logError)
        {
            var settings = new JsonSerializerSettings();

            settings.Error = (sender, err) =>
            {
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
                string data;

                var path = Path.Combine(BasePath, $"{prop.Name}.{Extension}");
                using (var file = File.OpenText(path))
                    data = await file.ReadToEndAsync();

                var sheet = Deserialize(data, prop.PropertyType, context.Logger) as Sheet;
                prop.SetValue(this, sheet);

                if (sheet != null)
                {
                    sheet.Name = prop.Name;
                    context.Container.AllSheets.Add(sheet);
                }
            }

            return true;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                var sheet = prop.GetValue(this);
                var data = Serialize(sheet, prop.PropertyType, context.Logger);

                var path = Path.Combine(BasePath, $"{prop.Name}.{Extension}");
                using (var file = File.CreateText(path))
                    await file.WriteAsync(data);
            }

            return true;
        }
    }
}
