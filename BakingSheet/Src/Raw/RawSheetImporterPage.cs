using System;
using System.Collections;
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

            ISheetRow sheetRow = null;

            for (int pageRow = 1; !page.IsEmptyRow(pageRow); ++pageRow)
            {
                var rowId = page.GetCell(0, pageRow);

                if (!string.IsNullOrEmpty(rowId))
                {
                    using (context.Logger.BeginScope(rowId))
                    {
                        sheetRow = Activator.CreateInstance(sheet.RowType) as ISheetRow;

                        page.ImportToObject(importer, context, sheetRow, pageRow);

                        if (sheet.Contains(sheetRow.Id))
                        {
                            context.Logger.LogError("Already has row with id \"{RowId}\"", sheetRow.Id);
                            sheetRow = null;
                        }
                        else
                        {
                            sheet.Add(sheetRow);
                        }
                    }
                }

                if (sheetRow is ISheetRowArray sheetRowArray)
                {
                    using (context.Logger.BeginScope(sheetRow.Id))
                    using (context.Logger.BeginScope(sheetRowArray.Arr.Count))
                    {
                        var sheetElem = Activator.CreateInstance(sheetRowArray.ElemType);

                        page.ImportToObject(importer, context, sheetElem, pageRow);

                        sheetRowArray.Arr.Add(sheetElem);
                    }
                }
            }
        }

        private static void ImportToObject(this IRawSheetImporterPage page, RawSheetImporter importer, SheetConvertingContext context, object obj, int pageRow)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

            for (int pageColumn = 0; !page.IsEmptyCell(pageColumn, 0); ++pageColumn)
            {
                var columnValue = page.GetCell(pageColumn, 0);

                using (context.Logger.BeginScope(columnValue))
                {
                    var cellValue = page.GetCell(pageColumn, pageRow);
                    if (string.IsNullOrEmpty(cellValue))
                        continue;

                    var prop = type.GetProperty(columnValue, bindingFlags);
                    if (prop == null)
                        continue;

                    try
                    {
                        object value = importer.StringToValue(prop.PropertyType, cellValue);
                        prop.SetValue(obj, value);
                    }
                    catch (Exception ex)
                    {
                        context.Logger.LogError(ex, "Failed to convert value \"{CellValue}\" of type {PropertyType}", cellValue, prop.PropertyType);
                    }
                }
            }
        }
    }
}
