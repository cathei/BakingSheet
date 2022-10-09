// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Raw
{
    /// <summary>
    /// Single page of a Spreadsheet workbook for exporting.
    /// </summary>
    public interface IRawSheetExporterPage
    {
        void SetCell(int col, int row, string data);
    }

    public static class RawSheetExporterPageExtensions
    {
        // TODO: in .net standard 2.1 this is not needed
        private static readonly string[] Delimiter = { Config.Delimiter };

        public static void Export(this IRawSheetExporterPage page,
            RawSheetConverter exporter, SheetConvertingContext context, ISheet sheet)
        {
            var propertyMap = sheet.GetPropertyMap(context);
            var resolver = context.Container.ContractResolver;

            propertyMap.UpdateIndex(sheet);

            var leafs = propertyMap.TraverseLeaf();

            int pageColumn = 0;

            var valueContext = new SheetValueConvertingContext(exporter, resolver);

            List<string> headerRows = new List<string>();
            object[] arguments = new object[propertyMap.MaxDepth];

            foreach (var (node, indexes) in leafs)
            {
                int i = 0;

                foreach (var index in indexes)
                {
                    var arg = valueContext.ValueToString(index.GetType(), index);
                    arguments[i++] = arg;
                }

                var columnName = string.Format(node.FullPath, arguments);

                if (exporter.SplitHeader)
                {
                    int tempRow = 0;

                    foreach (var path in columnName.Split(Delimiter, StringSplitOptions.None))
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
                int maxVerticalCount = 1;

                pageColumn = 0;

                foreach (var (node, indexes) in leafs)
                {
                    int verticalCount = node.GetVerticalCount(sheetRow, indexes.GetEnumerator());

                    for (int vindex = 0; vindex < verticalCount; ++vindex)
                    {
                        var value = node.GetValue(sheetRow, vindex, indexes.GetEnumerator());
                        var valueString = valueContext.ValueToString(node.ValueType, value);
                        page.SetCell(pageColumn, pageRow + vindex, valueString);
                    }

                    if (maxVerticalCount < verticalCount)
                        maxVerticalCount = verticalCount;

                    pageColumn++;
                }

                pageRow += maxVerticalCount;
            }
        }
    }
}
