// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    /// <summary>
    /// Generic sheet converter for cell-based Spreadsheet sources.
    /// </summary>
    public abstract class RawSheetConverter : RawSheetImporter, ISheetConverter
    {
        public bool SplitHeader { get; set; }

        protected abstract Task<bool> SaveData();
        protected abstract IRawSheetExporterPage CreatePage(string sheetName);

        protected RawSheetConverter(TimeZoneInfo timeZoneInfo, IFormatProvider formatProvider, bool splitHeader = false)
            : base(timeZoneInfo, formatProvider)
        {
            SplitHeader = splitHeader;
        }

        public async Task<bool> Export(SheetConvertingContext context)
        {
            foreach (var pair in context.Container.GetSheetProperties())
            {
                using (context.Logger.BeginScope(pair.Key))
                {
                    var sheet = pair.Value.GetValue(context.Container) as ISheet;
                    if (sheet == null)
                        continue;

                    var page = CreatePage(sheet.Name);
                    ExportPage(page, context, sheet);
                }
            }

            var success = await SaveData();

            if (!success)
            {
                context.Logger.LogError("Failed to save data");
                return false;
            }

            return true;
        }


        private void ExportPage(IRawSheetExporterPage page, SheetConvertingContext context, ISheet sheet)
        {
            var propertyMap = sheet.GetPropertyMap(context);
            var resolver = context.Container.ContractResolver;

            propertyMap.UpdateIndex(sheet);

            var leafs = propertyMap.TraverseLeaf();

            int pageColumn = 0;

            var valueContext = new SheetValueConvertingContext(this, resolver);

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

                if (SplitHeader)
                {
                    int tempRow = 0;

                    foreach (var path in columnName.Split(Config.IndexDelimiterArray, StringSplitOptions.None))
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

            int pageRow = SplitHeader ? headerRows.Count : 1;

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

                        string valueString = null;
                        if (value != null)
                            valueString = node.ValueConverter.ValueToString(node.ValueType, value, valueContext);

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
