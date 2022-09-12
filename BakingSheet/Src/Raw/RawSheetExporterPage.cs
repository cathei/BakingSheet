// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

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
        public static void Export(this IRawSheetExporterPage page, RawSheetConverter exporter, SheetConvertingContext context, ISheet sheet)
        {
            PropertyMap propertyMap = new PropertyMap(context, sheet.GetType(), exporter.IsConvertableNode);

            propertyMap.UpdateIndex(sheet);

            var leafs = propertyMap.TraverseLeaf();

            int pageColumn = 0;

            List<string> headerRows = new List<string>();

            // TODO: in .net standard 2.1 this is not needed
            var delimiter = new string[] { Config.Delimiter };

            foreach (var (node, indexes) in leafs)
            {
                var arguments = indexes.Select(x => exporter.ValueToString(x.GetType(), x)).ToArray();
                var columnName = string.Format(node.FullPath, arguments);

                if (exporter.SplitHeader)
                {
                    int tempRow = 0;

                    foreach (var path in columnName.Split(delimiter, StringSplitOptions.None))
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
                        var valueString = exporter.ValueToString(node.ValueType, value);
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
