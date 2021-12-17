using System;
using System.IO;
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
        private TestLogger _logger;
        private TestSheetContainer _container;
        private CsvSheetConverter _converter;

        public CsvImportTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
            _converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public async Task TestImportEmptyCsv()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Tests.csv"), "Id");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
            Assert.Equal(nameof(TestSheetContainer.Tests), _container.Tests.Name);
        }

        [Theory]
        [InlineData("Id,@!$@!$! \"1!$@ 2,\n,,xxx,")]
        public async Task TestImportMalformedCsv(string content)
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Tests.csv"), content);

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
        }

        [Fact]
        public async Task TestImportMissingColumn()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Tests.csv"), "");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
            Assert.Equal(nameof(TestSheetContainer.Tests), _container.Tests.Name);

            _logger.VerifyLog(LogLevel.Error,
                "First column \"(null)\" must be named \"Id\"",
                new [] { "Tests" });
        }

        [Fact]
        public async Task TestImportWrongEnum()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn\nWrongEnum,1");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Single(_container.Types);

            _logger.VerifyLog(LogLevel.Error,
                "Failed to convert value \"WrongEnum\" of type Cathei.BakingSheet.Tests.TestEnum",
                new [] { "Types", "WrongEnum", "Id" });
        }

        [Fact]
        public async Task TestImportDuplicatedRow()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn\nAlpha,1\nCharlie,2\nAlpha,3\nBravo,4");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Equal(3, _container.Types.Count);
            Assert.Equal(1, _container.Types[TestEnum.Alpha].IntColumn);
            Assert.Equal(4, _container.Types[TestEnum.Bravo].IntColumn);

            _logger.VerifyLog(LogLevel.Error,
                "Already has row with id \"Alpha\"",
                new [] { "Types", "Alpha" });
        }

        [Fact]
        public async Task TestImportNestedCsv()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Nested.csv"), "Id,Struct.1.X,Struct.1.Y,Struct.1.Z.1,Struct.1.Z.2,Struct.2.X,Struct.2.Y,Struct.2.Z.1,Struct.2.Z.2,IntList.1,IntList.2,IntList.3,IntList.4,IntList.5\nRow1,,,,,,,,,1,2,3,,\n,,,,,,,,,4,5,6,7,8\nRow2,,,,,,,,\nRow3,1,10,a,b,2,20,c,,,,,,\n");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Equal(3, _container.Nested.Count);
            Assert.Equal(2, _container.Nested["Row1"].Count);
            Assert.Equal(3, _container.Nested["Row1"][0].IntList.Count);
            Assert.Equal(2, _container.Nested["Row3"].Struct.Count);
            Assert.Null(_container.Nested["Row1"].Struct);
        }
    }
}