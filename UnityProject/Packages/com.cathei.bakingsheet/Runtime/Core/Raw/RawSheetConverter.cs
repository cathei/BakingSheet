// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetConverter : RawSheetImporter, ISheetConverter
    {
        public bool SplitHeader { get; set; }

        protected abstract Task<bool> SaveData();
        protected abstract IRawSheetExporterPage CreatePage(string sheetName);

        protected RawSheetConverter(TimeZoneInfo timeZoneInfo, IFormatProvider formatProvider, bool splitHeader = false)
            : base(timeZoneInfo, formatProvider)
        {
            SplitHeader = splitHeader;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            foreach (var prop in context.Container.GetSheetProperties())
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container) as ISheet;
                    if (sheet == null)
                        continue;

                    var page = CreatePage(sheet.Name);
                    page.Export(this, context, sheet);
                }
            }

            var success = await SaveData();

            if (!success)
            {
                context.Logger.LogError("Failed to save data");
                return false;
            }

            return true;
        }
    }
}
