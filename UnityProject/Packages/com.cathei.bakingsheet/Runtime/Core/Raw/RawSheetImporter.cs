// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Globalization;
using System.Threading.Tasks;
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

        public RawSheetImporter(TimeZoneInfo? timeZoneInfo, IFormatProvider? formatProvider)
        {
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }

        public virtual void Reset()
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
                    var sheet = Activator.CreateInstance(pair.Value.PropertyType) as ISheet;

                    if (sheet == null)
                    {
                        context.Logger.LogError("Failed to create sheet of type {SheetType}", pair.Value.PropertyType);
                        continue;
                    }

                    page.Import(this, context, sheet);
                    pair.Value.SetValue(context.Container, sheet);
                }
            }

            return true;
        }
    }
}
