// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using NReco.Csv;

namespace Cathei.BakingSheet
{
    public class CsvSheetConverter : RawSheetConverter
    {
        private IFileSystem _fileSystem;
        private string _loadPath;
        private string _extension;
        private Dictionary<string, Page> _pages = new Dictionary<string, Page>();

        private class CsvTable : List<List<string>>
        {
            public List<string> AddRow()
            {
                var row = new List<string>();
                Add(row);
                return row;
            }
        }

        public CsvSheetConverter(string loadPath, TimeZoneInfo timeZoneInfo = null, string extension = "csv", IFileSystem fileSystem = null, bool splitHeader = false, IFormatProvider formatProvider = null)
            : base(timeZoneInfo, formatProvider, splitHeader)
        {
            _loadPath = loadPath;
            _extension = extension;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        private class Page : IRawSheetImporterPage, IRawSheetExporterPage
        {
            private CsvTable _table;

            public string SubName => null;

            public CsvTable Table => _table;

            public Page(CsvTable table)
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

            public void SetCell(int col, int row, string data)
            {
                for (int i = _table.Count; i <= row; ++i)
                    _table.AddRow();

                for (int i = _table[row].Count; i <= col; ++i)
                    _table[row].Add(null);

                _table[row][col] = data;
            }
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            if (_pages.TryGetValue(sheetName, out var page))
                yield return page;
        }

        protected override IRawSheetExporterPage CreatePage(string sheetName)
        {
            var page = new Page(new CsvTable());
            return _pages[sheetName] = page;
        }

        protected override Task<bool> LoadData()
        {
            var files = _fileSystem.GetFiles(_loadPath, _extension);

            _pages.Clear();

            foreach (var file in files)
            {
                using (var stream = _fileSystem.OpenRead(file))
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

                    _pages[Path.GetFileNameWithoutExtension(file)] = new Page(table);
                }
            }

            return Task.FromResult(true);
        }

        protected override Task<bool> SaveData()
        {
            _fileSystem.CreateDirectory(_loadPath);

            foreach (var pageItem in _pages)
            {
                var file = Path.Combine(_loadPath, $"{pageItem.Key}.{_extension}");

                using (var stream = _fileSystem.OpenWrite(file))
                using (var writer = new StreamWriter(stream))
                {
                    var csv = new CsvWriter(writer);
                    var page = pageItem.Value;

                    foreach (var row in page.Table)
                    {
                        foreach (var cell in row)
                            csv.WriteField(cell);
                        csv.NextRecord();
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}
