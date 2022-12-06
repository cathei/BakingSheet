// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Represents a Container, workbook that holds multiple Sheet.
    /// </summary>
    public abstract class SheetContainerBase
    {
        private readonly ILogger _logger;
        private Dictionary<string, PropertyInfo> _sheetProperties;

        public virtual ISheetContractResolver ContractResolver => SheetContractResolver.Instance;

        protected SheetContainerBase(ILogger logger)
        {
            _logger = logger;
        }

        public IReadOnlyDictionary<string, PropertyInfo> GetSheetProperties()
        {
            if (_sheetProperties == null)
            {
                _sheetProperties = Config.GetEligibleProperties(GetType())
                    .Where(p => typeof(ISheet).IsAssignableFrom(p.PropertyType))
                    .ToDictionary(x => x.Name);
            }

            return _sheetProperties;
        }

        public ISheet Find(string name)
        {
            var props = GetSheetProperties();

            if (props.TryGetValue(name, out var sheet))
                return sheet.GetValue(this) as ISheet;

            return null;
        }

        public T Find<T>(string name) where T : class, ISheet
        {
            return Find(name) as T;
        }

        public async Task<bool> Bake(params ISheetImporter[] importers)
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            foreach (var prop in GetSheetProperties().Values)
            {
                // clear currently assigned sheets
                prop.SetValue(this, null);
            }

            foreach (var importer in importers)
            {
                var success = await importer.Import(context);

                if (!success)
                    return false;
            }

            PostLoad();

            return true;
        }

        public async Task<bool> Store(ISheetExporter exporter)
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var success = await exporter.Export(context);
            return success;
        }

        public virtual void PostLoad()
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var properties = GetSheetProperties();

            var rowTypeToSheet = new Dictionary<Type, ISheet>(properties.Count);

            foreach (var pair in properties)
            {
                var sheet = pair.Value.GetValue(this) as ISheet;

                if (sheet == null)
                {
                    context.Logger.LogError("Failed to find sheet: {SheetName}", pair.Key);
                    continue;
                }

                sheet.Name = pair.Key;

                if (rowTypeToSheet.ContainsKey(sheet.RowType))
                {
                    // row type must be unique in a sheet container
                    context.Logger.LogError("Duplicated Row type is used for {SheetName}", pair.Key);
                    continue;
                }

                rowTypeToSheet.Add(sheet.RowType, sheet);
            }

            // making sure all references are mapped before calling PostLoad
            foreach (var sheet in rowTypeToSheet.Values)
                sheet.MapReferences(context, rowTypeToSheet);

            foreach (var sheet in rowTypeToSheet.Values)
                sheet.PostLoad(context);
        }

        public virtual void Verify(params SheetVerifier[] verifiers)
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
                Verifiers = verifiers
            };

            foreach (var pair in GetSheetProperties())
            {
                if (pair.Value.GetValue(this) is ISheet sheet)
                {
                    sheet.VerifyAssets(context);
                }
            }
        }
    }
}
