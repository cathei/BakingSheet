using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cathei.BakingSheet
{
    public class ExcelSheetImporter : ISheetImporter
    {
        private string _loadPath;
        private string _searchPattern;
        private Dictionary<string, DataTable> _dataTables;

        public ExcelSheetImporter(string loadPath, string searchPattern = "*.xlsx")
        {
            _loadPath = loadPath;
            _searchPattern = searchPattern;
        }

        private class Data : ISheetImporterData
        {
            private DataTable _table;

            public Data(DataTable table)
            {
                _table = table;
            }

            public string GetCell(int col, int row)
            {
                if (col >= _table.Columns.Count || row >= _table.Rows.Count)
                    return null;

                return _table.Rows[row][col].ToString();
            }
        }

        public Task<bool> Load()
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

        public ISheetImporterData GetData(string sheetName)
        {
            if (_dataTables.TryGetValue(sheetName, out var table))
                return new Data(table);
            return null;
        }
    }
}
