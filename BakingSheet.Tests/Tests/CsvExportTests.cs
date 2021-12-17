using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class CsvExportTests
    {
        private TestFileSystem _fileSystem;
        private TestLogger _logger;
        private TestSheetContainer _container;
        private CsvSheetConverter _converter;

        public CsvExportTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
            _converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }

        [Fact]
        public async Task TestExportEmptyCsv()
        {
            _container.Tests = new TestSheet();
            _container.Arrays = new TestArraySheet();

            _container.PostLoad();

            var result = await _container.Store(_converter);

            Assert.True(result);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Tests.csv"), "Id,Content\n");
            _fileSystem.VerifyTestData(Path.Combine("testdata", "Arrays.csv"), "Id,Content,ElemContent\n");
        }

        [Fact]
        public async Task TestExportSampleCsv()
        {
            _container.Tests = new TestSheet();
            
            var testRow = new TestSheet.Row {
                Id = "TestId",
                Content = "TestContent"
            };

            _container.Tests.Add(testRow);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Tests.csv"), "Id,Content\nTestId,TestContent\n");
        }

        [Fact]
        public async Task TestExportArrayCsv()
        {
            _container.Arrays = new TestArraySheet();

            var arrayRow = new TestArraySheet.Row {
                Id = "TestId",
                Content = "TestContent"
            };

            var arrayElem1 = new TestArraySheet.Elem {
                ElemContent = "TestElemContent1"
            };

            var arrayElem2 = new TestArraySheet.Elem {
                ElemContent = "TestElemContent2"
            };

            arrayRow.Arr.Add(arrayElem1);
            arrayRow.Arr.Add(arrayElem2);

            _container.Arrays.Add(arrayRow);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Arrays.csv"), "Id,Content,ElemContent\nTestId,TestContent,TestElemContent1\n,,TestElemContent2\n");
        }
    }
}
