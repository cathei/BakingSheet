using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ImportUnitTests : IDisposable
    {
        private TestFileSystem _fileSystem;

        public ImportUnitTests()
        {
            _fileSystem = new TestFileSystem();
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public async Task TestImportEmptyCsv()
        {
            _fileSystem.SetTestData("testdata/Tests.csv", "Id");

            var logger = new NullLogger<ImportUnitTests>();
            var container = new TestSheetContainer(logger);
            var converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);

            var result = await container.Bake(converter);

            Assert.Equal(true, result);
            Assert.Equal(0, container.Tests.Count);
        }

        [Fact]
        public async Task TestImportMissingColumn()
        {
            _fileSystem.SetTestData("testdata/Tests.csv", "");

            var loggerMock = new Mock<ILogger>();

            var container = new TestSheetContainer(loggerMock.Object);
            var converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);

            var result = await container.Bake(converter);

            Assert.Equal(true, result);

            loggerMock.Verify(m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((s, _) => s.ToString() == "[Tests] First column must be named \"Id\""),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once);
        }

        [Fact]
        public void TestImportJson()
        {

        }
    }
}
