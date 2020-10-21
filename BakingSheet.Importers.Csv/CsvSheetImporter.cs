using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using NReco.Csv;

namespace Cathei.BakingSheet
{
    public class CsvSheetImporter : ISheetImporter
    {
        private string _loadPath;
        private string _searchPattern;
        private Dictionary<string, CsvTable> _dataTables;

        private class CsvTable : List<List<string>>
        {
            public List<string> AddRow()
            {
                var row = new List<string>();
                Add(row);
                return row;
            }
        }

        public CsvSheetImporter(string loadPath, string searchPattern = "*.csv")
        {
            _loadPath = loadPath;
            _searchPattern = searchPattern;
        }

        private class Data : ISheetImporterData
        {
            private CsvTable _table;

            public Data(CsvTable table)
            {
                _table = table;
            }

            public string GetCell(int col, int row)
            {
                if (row >= _table.Count)
                    return null;
                
                if (col >= _table[row].Count)
                    return null;

                return _table[row][col];
            }
        }

        public Task<bool> Load()
        {
            var files = Directory.GetFiles(_loadPath, _searchPattern);

            _dataTables = new Dictionary<string, CsvTable>();

            foreach (var file in files)
            {
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    var csv = new CsvReader(reader);
                    var table = new CsvTable();

                    while (csv.Read())
                    {
                        var row = table.AddRow();
                        for (int i = 0; i < csv.FieldsCount; ++i)
                            row.Add(csv[i]);
                    }

                    _dataTables[Path.GetFileNameWithoutExtension(file)] = table;
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
