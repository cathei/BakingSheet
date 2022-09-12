// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class SheetContainerBase
    {
        private ILogger _logger;
        private PropertyInfo[] _sheetProperties;

        public SheetContainerBase(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PropertyInfo> GetSheetProperties()
        {
            if (_sheetProperties == null)
            {
                _sheetProperties = GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => typeof(ISheet).IsAssignableFrom(p.PropertyType))
                    .ToArray();
            }

            return _sheetProperties;
        }

        public async Task<bool> Bake(params ISheetImporter[] importers)
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

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

            foreach (var prop in GetSheetProperties())
            {
                if (prop.GetValue(this) is ISheet sheet)
                {
                    sheet.Name = prop.Name;
                    sheet.PostLoad(context);
                }
                else
                {
                    context.Logger.LogError("Failed to find sheet: {SheetName}", prop.Name);
                }
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

            foreach (var prop in GetSheetProperties())
            {
                if (prop.GetValue(this) is ISheet sheet)
                {
                    sheet.VerifyAssets(context);
                }
            }
        }
    }
}
