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

            int pageColumn = -1;

            foreach ((var node, int offset, var indexes) in leafs)
            {
                if (offset == 0)
                {
                    pageColumn += 1;

                    var columnName = string.Format(node.FullPath, indexes);
                    page.SetCell(pageColumn, 0, columnName);
                }
            }

            int pageRow = 1;

            foreach (ISheetRow sheetRow in sheet)
            {
                pageColumn = -1;

                int usedRow = 0;

                using (context.Logger.BeginScope(sheetRow.Id))
                {
                    foreach ((var node, int offset, var indexes) in leafs)
                    {
                        if (offset == 0)
                            pageColumn += 1;

                        object value = node.Get(sheetRow, indexes);
                        string cellValue = exporter.ValueToString(node.Element, value);

                        page.SetCell(pageColumn, pageRow + offset, cellValue);

                        usedRow = Math.Max(usedRow, offset);
                    }
                }

                pageRow += usedRow + 1;
            }
        }
    }
}
