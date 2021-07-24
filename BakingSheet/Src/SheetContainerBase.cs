using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class SheetContainerBase
    {
        public bool IsLoaded { get; private set; }

        public List<ISheet> AllSheets = new List<ISheet>();

        private ILogger _logger;

        public SheetContainerBase(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PropertyInfo> GetSheetProperties()
        {
            return GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => typeof(ISheet).IsAssignableFrom(p.PropertyType));
        }

        public async Task<bool> Bake(ISheetImporter importer)
        {
            AllSheets.Clear();

            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var success = await importer.Import(context);

            if (!success)
                return false;

            PostLoad(context);

            IsLoaded = true;

            return true;
        }

        public async Task<bool> Store(ISheetExporter exporter)
        {
            if (!IsLoaded)
            {
                _logger.LogError("Sheet container is not loaded!");
                return false;
            }

            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var success = await exporter.Export(context);
            return success;
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
