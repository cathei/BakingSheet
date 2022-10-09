// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ValueConvertTests : IDisposable
    {
        private TestFileSystem _fileSystem;

        public ValueConvertTests()
        {
            _fileSystem = new TestFileSystem();
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        private enum TestEnum
        {
            Apple, Banana, Cherry, Durian
        }

        [SheetValueConverter(typeof(MyStructConverter))]
        private readonly record struct MyStruct(string MyString);

        private class MyStructConverter : SheetValueConverter<MyStruct>
        {
            protected override MyStruct StringToValue(Type type, string value, SheetValueConvertingContext context)
            {
                return new MyStruct(new string(value.Reverse().ToArray()));
            }

            protected override string ValueToString(Type type, MyStruct value, SheetValueConvertingContext context)
            {
                return new string(value.MyString.Reverse().ToArray());
            }
        }

        public static IEnumerable<object[]> GetStringToValueTestData()
        {
            yield return new object[] { typeof(int), "3", 3 };
            yield return new object[] { typeof(int?), "3", 3 };
            yield return new object[] { typeof(int?), null, null };
            yield return new object[] { typeof(float), "3.14", 3.14f };
            yield return new object[] { typeof(string), "abcd", "abcd" };
            yield return new object[] { typeof(TimeSpan), "20:00", new TimeSpan(20, 0, 0) };
            yield return new object[] { typeof(TimeSpan), "12:34:56", new TimeSpan(12, 34, 56) };
            yield return new object[] { typeof(TestEnum), "Durian", TestEnum.Durian };
            yield return new object[] { typeof(TestEnum?), "Banana", TestEnum.Banana };
            yield return new object[] { typeof(MyStruct), "Banana", new MyStruct("ananaB") };
        }

        [Theory]
        [MemberData(nameof(GetStringToValueTestData))]
        public void TestStringToValue(Type type, string data, object expected)
        {
            var converter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var value = context.StringToValue(type, data);

            Assert.Equal(expected, value);
        }

        public static IEnumerable<object[]> GetValueToStringTestData()
        {
            yield return new object[] { typeof(int), 3, "3" };
            yield return new object[] { typeof(int?), 3, "3" };
            yield return new object[] { typeof(int?), null, null };
            yield return new object[] { typeof(float), 3.14f, "3.14" };
            yield return new object[] { typeof(string), "abcd", "abcd" };
            yield return new object[] { typeof(TimeSpan), new TimeSpan(20, 0, 0), "20:00:00" };
            yield return new object[] { typeof(TimeSpan), new TimeSpan(12, 34, 56), "12:34:56" };
            yield return new object[] { typeof(TestEnum), TestEnum.Durian, "Durian" };
            yield return new object[] { typeof(TestEnum?), TestEnum.Banana, "Banana" };
            yield return new object[] { typeof(MyStruct), new MyStruct("ananaB"), "Banana" };
        }

        [Theory]
        [MemberData(nameof(GetValueToStringTestData))]
        public void TestValueToString(Type type, object value, string expected)
        {
            var converter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var data = context.ValueToString(type, value);

            Assert.Equal(expected, data);
        }

        public static IEnumerable<object[]> GetStringToDateTimeTestData()
        {
            yield return new object[] { typeof(DateTime), "2020-01-01", new DateTime(2020, 01, 01), TimeZoneInfo.Utc };
            yield return new object[] { typeof(DateTime), "8/22/1994 10:34:12 PM", new DateTime(1994, 8, 22, 22, 34, 12), TimeZoneInfo.Utc };
        }

        [Theory]
        [MemberData(nameof(GetStringToDateTimeTestData))]
        public void TestStringToDateTime(Type type, string data, object expected, TimeZoneInfo timeZoneInfo)
        {
            var converter = new CsvSheetConverter("csvdata", timeZoneInfo, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var value = context.StringToValue(type, data);

            Assert.Equal(expected, value);
        }

        public static IEnumerable<object[]> GetDateTimeToStringTestData()
        {
            yield return new object[] { typeof(DateTime), new DateTime(2020, 01, 01), "01/01/2020 00:00:00", TimeZoneInfo.Utc };
            yield return new object[] { typeof(DateTime), new DateTime(1994, 8, 22, 22, 34, 12), "08/22/1994 22:34:12", TimeZoneInfo.Utc };
        }

        [Theory]
        [MemberData(nameof(GetDateTimeToStringTestData))]
        public void TestDateTimeToString(Type type, object value, string expected, TimeZoneInfo timeZoneInfo)
        {
            var converter = new CsvSheetConverter("csvdata", timeZoneInfo, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var data = context.ValueToString(type, value);

            Assert.Equal(expected, data);
        }
    }
}
