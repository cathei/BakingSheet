using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> GetStringToValueTestData()
        {
            yield return new object[] { typeof(int), "3", 3 };
            yield return new object[] { typeof(float), "3.14", 3.14f };
            yield return new object[] { typeof(string), "abcd", "abcd" };
            yield return new object[] { typeof(DateTime), "2020-01-01", new DateTime(2020, 01, 01) };
            yield return new object[] { typeof(DateTime), "8/22/1994 10:34:12 PM", new DateTime(1994, 8, 22, 22, 34, 12) };
            yield return new object[] { typeof(TimeSpan), "20:00", new TimeSpan(20, 0, 0) };
            yield return new object[] { typeof(TimeSpan), "12:34:56", new TimeSpan(12, 34, 56) };
        }

        [Theory]
        [MemberData(nameof(GetStringToValueTestData))]
        public void TestStringToValue(Type type, string data, object expected)
        {
            var value = _converter.StringToValue(type, data);

            Assert.Equal(expected, value);
        }

        public static IEnumerable<object[]> GetValueToStringTestData()
        {
            yield return new object[] { typeof(int), 3, "3" };
            yield return new object[] { typeof(float), 3.14f, "3.14" };
            yield return new object[] { typeof(string), "abcd", "abcd" };
            yield return new object[] { typeof(DateTime), new DateTime(2020, 01, 01), "1/1/2020 12:00:00 AM" };
            yield return new object[] { typeof(DateTime), new DateTime(1994, 8, 22, 22, 34, 12), "8/22/1994 10:34:12 PM" };
            yield return new object[] { typeof(TimeSpan), new TimeSpan(20, 0, 0), "20:00:00" };
            yield return new object[] { typeof(TimeSpan), new TimeSpan(12, 34, 56), "12:34:56" };
        }

        [Theory]
        [MemberData(nameof(GetValueToStringTestData))]
        public void TestValueToString(Type type, object value, string expected)
        {
            var data = _converter.ValueToString(type, value);

            Assert.Equal(expected, data);
        }
    }
}
