using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class JsonImportTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private Mock<ILogger> _loggerMock;
        private TestSheetContainer _container;
        private JsonSheetConverter _converter;

        public JsonImportTests()
        {
            _fileSystem = new TestFileSystem();
            _loggerMock = new Mock<ILogger>();
            _container = new TestSheetContainer(_loggerMock.Object);
            _converter = new JsonSheetConverter("testdata", fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("!@#$RandomText")]
        public async Task TestImportMalformedJson(string content)
        {
            _fileSystem.SetTestData("testdata/Tests.json", content);

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Null(_container.Tests);
        }

        [Fact]
        public async Task TestImportEmptyJson()
        {
            _fileSystem.SetTestData("testdata/Tests.json", "[]");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Empty(_container.Tests);
            Assert.Equal(nameof(TestSheetContainer.Tests), _container.Tests.Name);
        }

        [Fact]
        public async Task TestImportWrongEnum()
        {
            _fileSystem.SetTestData("testdata/Types.json", "[{\"Id\":\"WrongEnum\",\"IntColumn\":345}]");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Single(_container.Types);
            Assert.Equal(TestEnum.Alpha, _container.Types[0].Id);
            Assert.Equal(345, _container.Types[0].IntColumn);

            _loggerMock.VerifyLog(LogLevel.Error, "Error converting value \"WrongEnum\" to type 'Cathei.BakingSheet.Tests.TestEnum'. Path '[0].Id', line 1, position 18.", Times.Once());
        }
    }
}
