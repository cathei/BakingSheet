using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            PropertyMap propertyMap = new PropertyMap(exporter, context, sheet);

            foreach (ISheetRow sheetRow in sheet)
            {
                propertyMap.UpdateCount(sheetRow);
            }

            var leafs = propertyMap.TraverseLeaf();

            int pageColumn = 0;

            foreach ((var node, bool array, var indexes) in leafs)
            {
                var columnName = string.Format(node.FullPath, indexes.Cast<object>().ToArray());

                page.SetCell(pageColumn, 0, columnName);
                pageColumn += 1;
            }

            int pageRow = 1;

            foreach (ISheetRow sheetRow in sheet)
            {
                pageColumn = 0;

                var sheetRowArray = sheetRow as ISheetRowArray;

                using (context.Logger.BeginScope(sheetRow.Id))
                {
                    foreach ((var node, bool array, var indexes) in leafs)
                    {
                        if (!array)
                        {
                            object value = node.Get(sheetRow, indexes);
                            string cellValue = exporter.ValueToString(node.Element, value);
                            page.SetCell(pageColumn, pageRow, cellValue);
                        }
                        else if (sheetRowArray != null)
                        {
                            for (int i = 0; i < sheetRowArray.Arr.Count; ++i)
                            {
                                // use 1-base for index
                                indexes[0] = i + 1;

                                object value = node.Get(sheetRow, indexes);
                                string cellValue = exporter.ValueToString(node.Element, value);
                                page.SetCell(pageColumn, pageRow + i, cellValue);
                            }

                        }

                        pageColumn += 1;
                    }

                    if (sheetRowArray != null && sheetRowArray.Arr.Count > 1)
                        pageRow += sheetRowArray.Arr.Count - 1;
                    pageRow += 1;
                }

            }
        }
    }
}
