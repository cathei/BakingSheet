using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public class RawSheet
    {
        public List<RawSheetRow> Rows { get; }

        public RawSheet(RawSheetImporterPage data)
        {
            Rows = new List<RawSheetRow>();

            Init(data);
        }

        private void Init(RawSheetImporterPage data)
        {
            data.GetSize(out int numColumns, out int numRows);

            RawSheetRow dataRow = null;

            for (int row = 1; row < numRows + 1; ++row)
            {
                var rowId = data.GetCell(0, row);

                if (!string.IsNullOrEmpty(rowId))
                {
                    dataRow = new RawSheetRow();
                    Rows.Add(dataRow);
                }

                var dict = new Dictionary<string, string>();

                for (int col = 0; col < numColumns; ++col)
                {
                    var columnName = data.GetCell(col, 0);
                    var valueStr = data.GetCell(col, row);

                    if (!string.IsNullOrEmpty(valueStr))
                        dict.Add(columnName, valueStr);
                }

                dataRow.Add(dict);
            }
        }

        internal void WriteToSheet(RawSheetImporter importer, SheetConvertingContext context, Sheet sheet)
        {
            foreach (var rawRow in Rows)
            {
                try
                {
                    context.SetTag(sheet.Name);

                    var row = Activator.CreateInstance(sheet.RowType) as ISheetRow;
                    rawRow.WriteToSheetRow(importer, context, row);
                    (sheet as IDictionary).Add(row.Id, row);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
