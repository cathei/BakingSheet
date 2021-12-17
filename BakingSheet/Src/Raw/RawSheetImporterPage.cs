using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static bool IsEmptyRow(this IRawSheetImporterPage page, int row)
        {
            for (int col = 0; !page.IsEmptyCell(col, 0); ++col)
            {
                if (!page.IsEmptyCell(col, row))
                    return false;
            }

            return true;
        }

        public static void Import(this IRawSheetImporterPage page, RawSheetImporter importer, SheetConvertingContext context, ISheet sheet)
        {
            var idColumnName = page.GetCell(0, 0);
 
            if (idColumnName != nameof(ISheetRow.Id))
            {
                context.Logger.LogError("First column \"{ColumnName}\" must be named \"Id\"", idColumnName);
                return;
            }

            PropertyMap propertyMap = new PropertyMap(importer, context, sheet);

            ISheetRow sheetRow = null;
            string rowId = null;
            int sameRow = 0;

            for (int pageRow = 1; !page.IsEmptyRow(pageRow); ++pageRow)
            {
                string idCellValue = page.GetCell(0, pageRow);

                if (!string.IsNullOrEmpty(idCellValue))
                {
                    rowId = idCellValue;
                    sheetRow = Activator.CreateInstance(sheet.RowType) as ISheetRow;
                    sameRow = 0;
                }

                using (context.Logger.BeginScope(rowId))
                {
                    for (int pageColumn = 0; !page.IsEmptyCell(pageColumn, 0); ++pageColumn)
                    {
                        var columnValue = page.GetCell(pageColumn, 0);

                        using (context.Logger.BeginScope(columnValue))
                        {
                            string cellValue = page.GetCell(pageColumn, pageRow);
                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            propertyMap.SetValue(sheetRow, sameRow, columnValue, cellValue);
                        }
                    }

                    if (sameRow == 0)
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

                    sameRow += 1;
                }
            }
        }
    }
}
