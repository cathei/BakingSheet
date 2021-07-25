using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class CsvImportTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private Mock<ILogger> _loggerMock;
        private TestSheetContainer _container;
        private CsvSheetConverter _converter;

        public CsvImportTests()
        {
            _fileSystem = new TestFileSystem();
            _loggerMock = new Mock<ILogger>();
            _container = new TestSheetContainer(_loggerMock.Object);
            _converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public async Task TestImportEmptyCsv()
        {
            _fileSystem.SetTestData("testdata/Tests.csv", "Id");

            var result = await _container.Bake(_converter);

            Assert.Equal(true, result);
            Assert.Equal(0, _container.Tests.Count);
        }

        [Fact]
        public async Task TestImportMissingColumn()
        {
            _fileSystem.SetTestData("testdata/Tests.csv", "");

            var result = await _container.Bake(_converter);

            Assert.Equal(true, result);
            Assert.Equal(0, _container.Tests.Count);

            _loggerMock.VerifyLog(LogLevel.Error, "[Tests] First column must be named \"Id\"", Times.Once());
        }
    }
}
