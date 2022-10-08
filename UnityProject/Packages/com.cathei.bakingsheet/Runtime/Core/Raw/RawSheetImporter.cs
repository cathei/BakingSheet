﻿// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    /// <summary>
    /// Generic sheet importer for cell-based Spreadsheet sources.
    /// </summary>
    public abstract class RawSheetImporter : ISheetImporter, ISheetFormatter
    {
        protected abstract Task<bool> LoadData();
        protected abstract IRawSheetImporterPage GetPage(string sheetName);

        public TimeZoneInfo TimeZoneInfo { get; }
        public IFormatProvider FormatProvider { get; }

        private bool _isLoaded;

        public RawSheetImporter(TimeZoneInfo timeZoneInfo, IFormatProvider formatProvider)
        {
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }

        [UsedImplicitly] public virtual void Reset()
        {
            _isLoaded = false;
        }

        public async Task<bool> Import(SheetConvertingContext context)
        {
            if (!_isLoaded)
            {
                var success = await LoadData();

                if (!success)
                {
                    context.Logger.LogError("Failed to load data");
                    return false;
                }

                _isLoaded = true;
            }

            foreach (var pair in context.Container.GetSheetProperties())
            {
                using (context.Logger.BeginScope(pair.Key))
                {
                    var page = GetPage(pair.Key);

                    if (page == null)
                        continue;

                    var sheet = Activator.CreateInstance(pair.Value.PropertyType) as ISheet;

                    page.Import(this, context, sheet);
                    pair.Value.SetValue(context.Container, sheet);
                }
            }

            return true;
        }
    }
}
