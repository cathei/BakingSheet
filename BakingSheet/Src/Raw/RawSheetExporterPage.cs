using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Raw
{
    public interface IRawSheetExporterPage
    {
        void SetCell(int col, int row, string data);
    }

    public static class RawSheetExporterPageExtensions
    {
        private static bool ShouldExport(PropertyInfo prop)
        {
            if (prop.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;

            if (prop.SetMethod == null)
                return false;

            return true;
        }

        public static void Export(this IRawSheetExporterPage page, RawSheetConverter exporter, SheetConvertingContext context, ISheet sheet)
        {
            PropertyMap propertyMap = new PropertyMap(context, sheet.GetType(), Config.IsConvertable);

            propertyMap.UpdateIndex(sheet);

            var leafs = propertyMap.TraverseLeaf();

            int pageColumn = 0;

            List<string> headerRows = new List<string>();

            foreach ((var node, bool array, var indexes) in leafs)
            {
                var arguments = indexes.Select(x => exporter.ValueToString(x.GetType(), x)).ToArray();
                var columnName = string.Format(node.FullPath, arguments);

                if (exporter.SplitHeader)
                {
                    int tempRow = 0;

                    foreach (var path in columnName.Split(Config.Delimiter))
                    {
                        while (headerRows.Count <= tempRow)
                            headerRows.Add(null);

                        if (headerRows[tempRow] != path)
                        {
                            headerRows[tempRow] = path;
                            page.SetCell(pageColumn, tempRow, path);
                        }

                        tempRow++;
                    }
                }
                else
                {
                    page.SetCell(pageColumn, 0, columnName);
                }

                pageColumn++;
            }

            int pageRow = exporter.SplitHeader ? headerRows.Count : 1;

            foreach (ISheetRow sheetRow in sheet)
            {
                pageColumn = 0;

                var sheetRowArray = sheetRow as ISheetRowArray;

                using (context.Logger.BeginScope(sheetRow.Id))
                {
                    foreach ((var node, bool isArray, var indexes) in leafs)
                    {
                        if (!isArray)
                        {
                            object value = node.GetValue(sheetRow, indexes.GetEnumerator());
                            string cellValue = exporter.ValueToString(node.ValueType, value);
                            page.SetCell(pageColumn, pageRow, cellValue);
                        }
                        else if (sheetRowArray != null)
                        {
                            for (int i = 0; i < sheetRowArray.Arr.Count; ++i)
                            {
                                // use 1-base for index
                                indexes[0] = i + 1;

                                object value = node.GetValue(sheetRow, indexes.GetEnumerator());
                                string cellValue = exporter.ValueToString(node.ValueType, value);
                                page.SetCell(pageColumn, pageRow + i, cellValue);
                            }
                        }

                        pageColumn++;
                    }

                    // if array count is 0 or 1 it is same as single row
                    if (sheetRowArray != null && sheetRowArray.Arr.Count > 1)
                        pageRow += sheetRowArray.Arr.Count - 1;

                    pageRow++;
                }
            }
        }
    }
}
