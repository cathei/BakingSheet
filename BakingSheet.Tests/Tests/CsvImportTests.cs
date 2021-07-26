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

            Assert.True(result);
            Assert.Empty(_container.Tests);
            Assert.Equal(nameof(TestSheetContainer.Tests), _container.Tests.Name);
        }

        [Theory]
        [InlineData("Id,@!$@!$! \"1!$@ 2,\n,,xxx,")]
        public async Task TestImportMalformedCsv(string content)
        {
            _fileSystem.SetTestData("testdata/Tests.csv", content);

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
        }

        [Fact]
        public async Task TestImportMissingColumn()
        {
            _fileSystem.SetTestData("testdata/Tests.csv", "");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
            Assert.Equal(nameof(TestSheetContainer.Tests), _container.Tests.Name);

            _loggerMock.VerifyLog(LogLevel.Error, "[Tests] First column \"\" must be named \"Id\"", Times.Once());
        }

        [Fact]
        public async Task TestImportWrongEnum()
        {
            _fileSystem.SetTestData("testdata/Types.csv", "Id,IntColumn\nWrongEnum,1");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Single(_container.Types);

            _loggerMock.VerifyLog(LogLevel.Error, "[Types>WrongEnum>Id] Failed to convert value \"WrongEnum\" of type Cathei.BakingSheet.Tests.TestEnum", Times.Once());
        }

        [Fact]
        public async Task TestImportDuplicatedRow()
        {
            _fileSystem.SetTestData("testdata/Types.csv", "Id,IntColumn\nAlpha,1\nCharlie,2\nAlpha,3\nBravo,4");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Equal(3, _container.Types.Count);
            Assert.Equal(1, _container.Types[TestEnum.Alpha].IntColumn);
            Assert.Equal(4, _container.Types[TestEnum.Bravo].IntColumn);

            _loggerMock.VerifyLog(LogLevel.Error, "[Types>Alpha] Already has row with id \"Alpha\"", Times.Once());
        }
    }
}