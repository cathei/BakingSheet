// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Raw;
using ExcelDataReader;

namespace Cathei.BakingSheet
{
    public class ExcelSheetConverter : RawSheetImporter
    {
        private string _loadPath;
        private string _extension;
        private Dictionary<string, List<Page>> _pages;
        private IFileSystem _fileSystem;

        public ExcelSheetConverter(string loadPath, TimeZoneInfo timeZoneInfo = null, string extension = "xlsx", IFileSystem fileSystem = null, IFormatProvider formatProvider = null)
            : base(timeZoneInfo, formatProvider)
        {
            _loadPath = loadPath;
            _extension = extension;
            _fileSystem = fileSystem ?? new FileSystem();
            _pages = new Dictionary<string, List<Page>>();
        }

        private class Page : IRawSheetImporterPage
        {
            private DataTable _table;

            public string SubName { get; }

            public Page(DataTable table, string subName)
            {
                _table = table;
                SubName = subName;
            }

            public string GetCell(int col, int row)
            {
                if (col >= _table.Columns.Count || row >= _table.Rows.Count)
                    return null;

                return _table.Rows[row][col].ToString();
            }
        }

        public override void Reset()
        {
            base.Reset();
            _pages.Clear();
        }

        protected override Task<bool> LoadData()
        {
            var files = _fileSystem.GetFiles(_loadPath, _extension);

            _pages.Clear();

            foreach (var file in files)
            {
                using (var stream = _fileSystem.OpenRead(file))
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
                        var tableName = table.TableName;

                        if (tableName.StartsWith(Config.Comment))
                            continue;

                        var (sheetName, subName) = Config.ParseSheetName(tableName);

                        if (!_pages.TryGetValue(sheetName, out var sheetList))
                        {
                            sheetList = new List<Page>();
                            _pages.Add(sheetName, sheetList);
                        }

                        sheetList.Add(new Page(table, subName));
                    }
                }
            }

            return Task.FromResult(true);
        }

        protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
        {
            if (_pages.TryGetValue(sheetName, out var page))
                return page;
            return Enumerable.Empty<IRawSheetImporterPage>();
        }
    }
}
