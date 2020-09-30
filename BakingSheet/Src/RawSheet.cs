using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class RawSheetRow : List<Dictionary<string, string>>
    {
        public override string ToString()
        {
            var infos = this.SelectMany(x => x)
                .GroupBy(x => x.Key)
                .Select(g => $"{g.Key}: {string.Join(",", g.Select(x => x.Value))}");

            return string.Join(",", infos);
        }
    }

    public class RawSheet
    {
        public List<RawSheetRow> Rows { get; }

        public RawSheet(ISheetImporterData data)
        {
            Rows = new List<RawSheetRow>();

            Init(data);
        }

        private void Init(ISheetImporterData data)
        {
            GetSize(data, out int numColumns, out int numRows);

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
 
        private void GetSize(ISheetImporterData data, out int numColumns, out int numRows)
        {
            int col = 0, row = 0;

            // First row is configuration variables
            // First column is data keys
            // Find number of configuration variables
            while (true)
            {
                var value = data.GetCell(col, 0);
                if (string.IsNullOrEmpty(value))
                    break;

                ++col;
            }

            numColumns = col;

            // Find number of data keys
            while (true)
            {
                bool isEmptyRow = Enumerable.Range(0, numColumns).All(i => {
                    var value = data.GetCell(i, row + 1);
                    return string.IsNullOrEmpty(value);
                });

                if (isEmptyRow)
                    break;

                ++row;
            }

            numRows = row;
        }
   }
}
