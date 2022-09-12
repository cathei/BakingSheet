// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public interface IRawSheetImporterPage
    {
        string GetCell(int col, int row);
    }

    public static class RawSheetImporterPageExtensions
    {
        public static bool IsEmptyCell(this IRawSheetImporterPage page, int col, int row)
        {
            return string.IsNullOrEmpty(page.GetCell(col, row));
        }

        /// <summary>
        /// If the row has no value in all valid column it count as empty row
        /// </summary>
        public static bool IsEmptyRow(this IRawSheetImporterPage page, int row)
        {
            for (int col = 0; IsValidColumn(page, col, row); ++col)
            {
                if (!page.IsEmptyCell(col, row))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// If the column has any value until current row, it count as valid column
        /// </summary>
        private static bool IsValidColumn(IRawSheetImporterPage page, int col, int row)
        {
            for (int prevRow = 0; prevRow <= row; ++prevRow)
            {
                if (!page.IsEmptyCell(col, prevRow))
                    return true;
            }

            return false;
        }

        public static void Import(this IRawSheetImporterPage page, RawSheetImporter importer, SheetConvertingContext context, ISheet sheet)
        {
            var idColumnName = page.GetCell(0, 0);

            if (idColumnName != nameof(ISheetRow.Id))
            {
                context.Logger.LogError("First column \"{ColumnName}\" must be named \"Id\"", idColumnName);
                return;
            }

            var columnNames = new List<string>();
            var headerRows = new List<string>();

            for (int pageRow = 0; pageRow == 0 || (page.IsEmptyCell(0, pageRow) && !page.IsEmptyRow(pageRow)); ++pageRow)
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

                columnNames.Add(string.Join(Config.Delimiter, headerRows.Take(lastValidRow + 1)));
            }

            PropertyMap propertyMap = new PropertyMap(context, sheet.GetType(), importer.IsConvertableNode);

            ISheetRow sheetRow = null;
            string rowId = null;
            int vindex = 0;
            bool skipRow = false;

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
                else if (skipRow)
                {
                    // skipping this row
                    continue;
                }

                using (context.Logger.BeginScope(rowId))
                {
                    try
                    {
                        ImportRow(page, importer, context, sheetRow, propertyMap, columnNames, vindex, pageRow);
                    }
                    catch
                    {
                        // failed to convert, skip this row
                        skipRow = true;
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

        private static void ImportRow(IRawSheetImporterPage page, RawSheetImporter importer, SheetConvertingContext context,
            ISheetRow sheetRow, PropertyMap propertyMap, List<string> columnNames, int vindex, int pageRow)
        {
            for (int pageColumn = 0; pageColumn < columnNames.Count; ++pageColumn)
            {
                string columnValue = columnNames[pageColumn];

                if (columnValue.StartsWith(Config.Comment))
                    continue;

                using (context.Logger.BeginScope(columnValue))
                {
                    string cellValue = page.GetCell(pageColumn, pageRow);
                    if (string.IsNullOrEmpty(cellValue))
                        continue;

                    try
                    {
                        propertyMap.SetValue(sheetRow, vindex, columnValue, cellValue, importer.StringToValue);
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
