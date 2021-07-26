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
                context.Logger.LogError($"[{context.Tag}] First column \"{idColumnName}\" must be named \"{nameof(ISheetRow.Id)}\"");
                return;
            }

            ISheetRow sheetRow = null;

            var parentTag = context.Tag;

            for (int pageRow = 1; !page.IsEmptyRow(pageRow); ++pageRow)
            {
                var rowId = page.GetCell(0, pageRow);

                if (!string.IsNullOrEmpty(rowId))
                {
                    sheetRow = Activator.CreateInstance(sheet.RowType) as ISheetRow;

                    context.SetTag(parentTag, rowId);

                    page.ImportToObject(importer, context, sheetRow, pageRow);

                    context.SetTag(parentTag, rowId);

                    if (sheet.Contains(sheetRow.Id))
                    {
                        context.Logger.LogError($"[{context.Tag}] Already has row with id \"{sheetRow.Id}\"");
                        sheetRow = null;
                    }
                    else
                    {
                        sheet.Add(sheetRow);
                    }
                }

                if (sheetRow is ISheetRowArray sheetRowArray)
                {
                    context.SetTag(parentTag, sheetRow.Id, sheetRowArray.Arr.Count);

                    var sheetElem = Activator.CreateInstance(sheetRowArray.ElemType);

                    page.ImportToObject(importer, context, sheetElem, pageRow);

                    sheetRowArray.Arr.Add(sheetElem);
                }
            }
        }

        private static void ImportToObject(this IRawSheetImporterPage page, RawSheetImporter importer, SheetConvertingContext context, object obj, int pageRow)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

            var parentTag = context.Tag;

            for (int pageColumn = 0; !page.IsEmptyCell(pageColumn, 0); ++pageColumn)
            {
                var columnValue = page.GetCell(pageColumn, 0);
                var cellValue = page.GetCell(pageColumn, pageRow);
                if (string.IsNullOrEmpty(cellValue))
                    continue;

                var prop = type.GetProperty(columnValue, bindingFlags);
                if (prop == null)
                    continue;

                context.SetTag(parentTag, columnValue);

                try
                {
                    object value = importer.StringToValue(context, prop.PropertyType, cellValue);
                    prop.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(ex, $"[{context.Tag}] Failed to convert value \"{cellValue}\" of type {prop.PropertyType}");
                }
            }
        }
    }
}
