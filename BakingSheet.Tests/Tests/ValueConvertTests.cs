using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ValueConvertTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private CsvSheetConverter _converter;

        public ValueConvertTests()
        {
            _fileSystem = new TestFileSystem();
            _converter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Theory]
        [InlineData(typeof(int), "3", 3)]
        [InlineData(typeof(string), "abcd", "abcd")]
        // [InlineData(typeof(DateTime), "2020-01-01", new DateTime(2020, 01, 01))]
        public void TestStringToValue(Type type, string data, object expected)
        {
            var value = _converter.StringToValue(type, data);

            Assert.Equal(expected, value);
        }
    }
}
