using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Cathei.BakingSheet.Raw;
using ExcelDataReader;

namespace Cathei.BakingSheet
{
    public class ExcelSheetConverter : RawSheetImporter
    {
        private string _loadPath;
        private string _searchPattern;
        private Dictionary<string, DataTable> _dataTables;

        public ExcelSheetConverter(string loadPath, TimeZoneInfo timeZoneInfo, string searchPattern = "*.xlsx")
            : base(timeZoneInfo)
        {
            _loadPath = loadPath;
            _searchPattern = searchPattern;
        }

        private class Page : RawSheetImporterPage
        {
            private DataTable _table;

            public Page(DataTable table)
            {
                _table = table;
            }

            public override string GetCell(int col, int row)
            {
                if (col >= _table.Columns.Count || row >= _table.Rows.Count)
                    return null;

                return _table.Rows[row][col].ToString();
            }
        }

        protected override Task<bool> LoadData()
        {
            var files = Directory.GetFiles(_loadPath, _searchPattern);

            _dataTables = new Dictionary<string, DataTable>();

            foreach (var file in files)
            {
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataset = reader.AsDataSet(new ExcelDataSetConfiguration {
                        UseColumnDataType = false,
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration {
                            UseHeaderRow = false,
                        }
                    });

                    for (int i = 0; i < dataset.Tables.Count; ++i)
                    {
                        var table = dataset.Tables[i];
                        if (!table.TableName.StartsWith("$"))
                            _dataTables.Add(table.TableName, table);
                    }
                }
            }

            return Task.FromResult(true);
        }

        protected override RawSheetImporterPage GetPage(string sheetName)
        {
            if (_dataTables.TryGetValue(sheetName, out var table))
                return new Page(table);
            return null;
        }
    }
}
