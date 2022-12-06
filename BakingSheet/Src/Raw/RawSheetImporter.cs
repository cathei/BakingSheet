// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    /// <summary>
    /// Generic sheet importer for cell-based Spreadsheet sources.
    /// </summary>
    public abstract class RawSheetImporter : ISheetImporter, ISheetFormatter
    {
        protected abstract Task<bool> LoadData();
        protected abstract IEnumerable<IRawSheetImporterPage> GetPages(string sheetName);

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

            foreach (var pair in context.Container.GetSheetProperties())
            {
                using (context.Logger.BeginScope(pair.Key))
                {
                    var pages = GetPages(pair.Key);
                    var sheet = pair.Value.GetValue(context.Container) as ISheet;

                    if (sheet == null)
                    {
                        // create new sheet
                        sheet = Activator.CreateInstance(pair.Value.PropertyType) as ISheet;
                        pair.Value.SetValue(context.Container, sheet);
                    }

                    if (sheet == null)
                    {
                        context.Logger.LogError("Failed to create sheet of type {SheetType}", pair.Value.PropertyType);
                        continue;
                    }

                    foreach (var page in pages.OrderBy(x => x.SubName))
                        ImportPage(page, context, sheet);
                }
            }

            return true;
        }

        private void ImportPage(IRawSheetImporterPage page, SheetConvertingContext context, ISheet sheet)
        {
            var idColumnName = page.GetCell(0, 0);

            if (idColumnName != nameof(ISheetRow.Id))
            {
                context.Logger.LogError("First column \"{ColumnName}\" must be named \"Id\"", idColumnName);
                return;
            }

            var columnNames = new List<string>();
            var headerRows = new List<string>();

            // first row is always header row
            headerRows.Add(null);

            // if id column is empty they are split header row
            for (int pageRow = 1; page.IsEmptyCell(0, pageRow) && !page.IsEmptyRow(pageRow); ++pageRow)
                headerRows.Add(null);

            for (int pageColumn = 0; ; ++pageColumn)
            {
                int lastValidRow = -1;

                for (int pageRow = 0; pageRow < headerRows.Count; ++pageRow)
                {
                    if (!page.IsEmptyCell(pageColumn, pageRow))
                    {
                        lastValidRow = pageRow;
                        headerRows[pageRow] = page.GetCell(pageColumn, pageRow);
                    }
                }

                if (lastValidRow == -1)
                    break;

                columnNames.Add(string.Join(Config.IndexDelimiter, headerRows.Take(lastValidRow + 1)));
            }

            PropertyMap propertyMap = sheet.GetPropertyMap(context);

            ISheetRow sheetRow = null;
            string rowId = null;
            int vindex = 0;

            for (int pageRow = headerRows.Count; !page.IsEmptyRow(pageRow); ++pageRow)
            {
                string idCellValue = page.GetCell(0, pageRow);

                if (!string.IsNullOrEmpty(idCellValue))
                {
                    if (idCellValue.StartsWith(Config.Comment))
                        continue;

                    rowId = idCellValue;
                    sheetRow = Activator.CreateInstance(sheet.RowType) as ISheetRow;
                    vindex = 0;
                }

                if (sheetRow == null)
                {
                    // skipping this row
                    continue;
                }

                using (context.Logger.BeginScope(rowId))
                {
                    try
                    {
                        ImportRow(page, context, sheetRow, propertyMap, columnNames, vindex, pageRow);
                    }
                    catch
                    {
                        // failed to convert, skip this row
                        sheetRow = null;
                        continue;
                    }

                    if (vindex == 0)
                    {
                        if (sheet.Contains(sheetRow.Id))
                        {
                            context.Logger.LogError("Already has row with id \"{RowId}\"", sheetRow.Id);
                        }
                        else
                        {
                            sheet.Add(sheetRow);
                        }
                    }

                    vindex++;
                }
            }
        }

        private void ImportRow(IRawSheetImporterPage page, SheetConvertingContext context, ISheetRow sheetRow,
            PropertyMap propertyMap, List<string> columnNames, int vindex, int pageRow)
        {
            for (int pageColumn = 0; pageColumn < columnNames.Count; ++pageColumn)
            {
                string columnValue = columnNames[pageColumn];

                if (columnValue.StartsWith(Config.Comment))
                    continue;

                using (context.Logger.BeginScope(columnValue))
                {
                    string cellValue = page.GetCell(pageColumn, pageRow);

                    // if cell is empty, value should not be set
                    // Property will keep it's default value
                    if (string.IsNullOrEmpty(cellValue))
                        continue;

                    try
                    {
                        propertyMap.SetValue(sheetRow, vindex, columnValue, cellValue, this);
                    }
                    catch (Exception ex)
                    {
                        // for Id column, throw and exclude whole column
                        if (pageColumn == 0)
                        {
                            context.Logger.LogError(ex, "Failed to set id \"{CellValue}\"", cellValue);
                            throw;
                        }

                        context.Logger.LogError(ex, "Failed to set value \"{CellValue}\"", cellValue);
                    }
                }
            }
        }

    }
}
