using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class SheetContainerBase
    {
        public bool IsLoaded { get; private set; }

        public List<Sheet> AllSheets = new List<Sheet>();

        private ILogger _logger;

        public SheetContainerBase(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Load(string loadPath, string ext = "json", Func<string, string> processor = null)
        {
            var sheetProps = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType.IsSubclassOf(typeof(Sheet)));

            var deserializeMethod = typeof(SheetUtility).GetMethod(
                nameof(SheetUtility.Deserialize),
                BindingFlags.Public | BindingFlags.Static);

            AllSheets.Clear();

            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            foreach (var prop in sheetProps)
            {
                string data;

                var path = Path.Combine(loadPath, $"{prop.Name}.{ext}");
                using (var file = File.OpenText(path))
                    data = await file.ReadToEndAsync();

                if (processor != null)
                    data = processor(data);

                var genericMethod = deserializeMethod.MakeGenericMethod(prop.PropertyType);
                var sheet = genericMethod.Invoke(null, new object[] { data, _logger }) as Sheet;
                prop.SetValue(this, sheet);

                if (sheet != null)
                {
                    sheet.Name = prop.Name;
                    AllSheets.Add(sheet);
                }
            }

            PostLoad(context);

            IsLoaded = true;
        }

        public async Task Store(string savePath, string ext = "json", Func<string, string> processor = null)
        {
            if (!IsLoaded)
            {
                _logger.LogError("Sheet container is not loaded!");
                return;
            }

            var sheetProps = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType.IsSubclassOf(typeof(Sheet)));

            var serializeMethod = typeof(SheetUtility).GetMethod(
                nameof(SheetUtility.Serialize),
                BindingFlags.Public | BindingFlags.Static);

            foreach (var prop in sheetProps)
            {
                var sheet = prop.GetValue(this);

                var genericMethod = serializeMethod.MakeGenericMethod(prop.PropertyType);
                var data = genericMethod.Invoke(null, new object[] { sheet, _logger }) as string;

                if (processor != null)
                    data = processor(data);

                var path = Path.Combine(savePath, $"{prop.Name}.{ext}");
                using (var file = File.CreateText(path))
                    await file.WriteAsync(data);
            }
        }

        public Task Bake(ISheetImporter importer)
        {
            return Bake(importer, TimeZoneInfo.Utc);
        }

        public async Task Bake(ISheetImporter importer, TimeZoneInfo timeZoneInfo)
        {
            AllSheets.Clear();

            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
                TimeZoneInfo = timeZoneInfo
            };

            var sheetProps = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType.IsSubclassOf(typeof(Sheet)));

            var success = await importer.Load();
            if (!success)
            {
                _logger.LogError($"Failed to import");
                return;
            }

            foreach (var prop in sheetProps)
            {
                var data = importer.GetData(prop.Name);
                if (data == null)
                {
                    _logger.LogError($"Failed to find sheet: {prop.Name}");
                    continue;
                }

                var rawSheet = new RawSheet(data);
                var sheet = Activator.CreateInstance(prop.PropertyType) as Sheet;
                sheet.ConvertFromRaw(rawSheet, context);
                prop.SetValue(this, sheet);

                sheet.Name = prop.Name;
                AllSheets.Add(sheet);
            }

            PostLoad(context);

            IsLoaded = true;
        }

        protected virtual void PostLoad(SheetConvertingContext context)
        {
            foreach (var sheet in AllSheets)
            {
                sheet.PostLoad(context);
            }
        }

        public virtual void Verify(params SheetVerifier[] verifiers)
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
                Verifiers = verifiers
            };

            foreach (var sheet in AllSheets)
            {
                sheet.VerifyAssets(context);
            }
        }
    }
}
