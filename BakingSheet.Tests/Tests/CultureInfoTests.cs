// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class CultureInfoTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private TestLogger _logger;
        private TestSheetContainer _container;

        public CultureInfoTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public async Task TestExportCultureInfoCsv()
        {
            _container.Types = new TestTypeSheet();

            var row1 = new TestTypeSheet.Row()
            {
                Id = TestEnum.Alpha,
                IntColumn = 876543210,
                FloatColumn = 123456.7891011f,
                DecimalColumn = 163025412.32m,
                DateTimeColumn = new DateTime(1994, 8, 22, 22, 34, 12),
                TimeSpanColumn = new TimeSpan(13, 05, 10),
                EnumColumn = TestEnum.Charlie
            };

            _container.Types.Add(row1);
            _container.PostLoad();

            var converter = new CsvSheetConverter(
                "testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem,
                formatProvider: CultureInfo.GetCultureInfo("fr-FR"));

            var result = await _container.Store(converter);

            _logger.VerifyNoError();

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn,FloatColumn,DecimalColumn,DateTimeColumn,TimeSpanColumn,EnumColumn\nAlpha,876543211,\"123456,79\",\"163025412,32\",22/08/1994 22:34:12,13:05:10,Charlie\n");
        }

        [Fact]
        public async Task TestImportCultureInfoCsv()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn,FloatColumn,DecimalColumn,DateTimeColumn,TimeSpanColumn\nAlpha,876543211,\"123456,79\",\"163025412,32\",22/08/1994 22:34:12,13:05:10\n");

            var converter = new CsvSheetConverter(
                "testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem,
                formatProvider: CultureInfo.GetCultureInfo("fr-FR"));

            await _container.Bake(converter);

            _logger.VerifyNoError();

            var row = _container.Types.Find(TestEnum.Alpha);

            Assert.Equal(876543210, row.IntColumn);
            Assert.Equal(123456.7891011f, row.FloatColumn);
            Assert.Equal(163025412.32m, row.DecimalColumn);
            Assert.Equal(new DateTime(1994, 8, 22, 22, 34, 12), row.DateTimeColumn);
            Assert.Equal(new TimeSpan(13, 05, 10), row.TimeSpanColumn);
        }
    }
}
