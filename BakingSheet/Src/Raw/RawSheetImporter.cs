// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
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

            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var page = GetPage(prop.Name);

                    if (page == null)
                        continue;

                    var sheet = Activator.CreateInstance(prop.PropertyType) as ISheet;

                    page.Import(this, context, sheet);
                    prop.SetValue(context.Container, sheet);
                }
            }

            return true;
        }
    }
}
