// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Unity;
using Newtonsoft.Json;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class AssetPathTests : IDisposable
    {
        private TestFileSystem _fileSystem;

        public AssetPathTests()
        {
            _fileSystem = new TestFileSystem();
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        public class TestPngAssetPath : AssetPath
        {
            public override string BasePath => "MyPath/";
            public override string Extension => ".png";
        }

        public static IEnumerable<object[]> AssetPathStringToValueTestData()
        {
            yield return new object[] { typeof(DirectAssetPath), "MyPng", "MyPng" };
            yield return new object[] { typeof(DirectAssetPath), "123", "123" };
            yield return new object[] { typeof(DirectAssetPath), "Nested/MyPng", "Nested/MyPng" };
            yield return new object[] { typeof(DirectAssetPath), null, null };
            yield return new object[] { typeof(DirectAssetPath), "", null };
            yield return new object[] { typeof(TestPngAssetPath), "MyPng", "MyPath/MyPng.png" };
            yield return new object[] { typeof(TestPngAssetPath), "123", "MyPath/123.png" };
            yield return new object[] { typeof(TestPngAssetPath), "Nested/MyPng", "MyPath/Nested/MyPng.png" };
            yield return new object[] { typeof(TestPngAssetPath), null, null };
            yield return new object[] { typeof(TestPngAssetPath), "", null };
        }

        [Theory]
        [MemberData(nameof(AssetPathStringToValueTestData))]
        public void TestStringToValue(Type type, string data, string expected)
        {
            var converter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var value = (ISheetAssetPath)context.StringToValue(type, data);

            Assert.Equal(expected, value.FullPath);
        }

        [Theory]
        [MemberData(nameof(AssetPathStringToValueTestData))]
        public void TestStringToValueJson(Type type, string data, string expected)
        {
            if (data == null)
                data = "null";
            else
                data = $"\"{data}\"";

            var value = (ISheetAssetPath)JsonConvert.DeserializeObject(
                data, type, new JsonSheetAssetPathConverter());

            Assert.Equal(expected, value.FullPath);
        }

        public static IEnumerable<object[]> AssetPathValueToStringTestData()
        {
            yield return new object[] { new DirectAssetPath { RawValue = "MyPng" }, "MyPng" };
            yield return new object[] { new DirectAssetPath { RawValue = "123" }, "123" };
            yield return new object[] { new DirectAssetPath { RawValue = "Nested/MyPng" }, "Nested/MyPng" };
            yield return new object[] { new DirectAssetPath { RawValue = null }, null };
            yield return new object[] { new DirectAssetPath { RawValue = "" }, null };
            yield return new object[] { new TestPngAssetPath { RawValue = "MyPng" }, "MyPng" };
            yield return new object[] { new TestPngAssetPath { RawValue = "123" }, "123" };
            yield return new object[] { new TestPngAssetPath { RawValue = "Nested/MyPng" }, "Nested/MyPng"};
            yield return new object[] { new TestPngAssetPath { RawValue = null }, null };
            yield return new object[] { new TestPngAssetPath { RawValue = "" }, null };
        }

        [Theory]
        [MemberData(nameof(AssetPathValueToStringTestData))]
        public void TestValueToString(ISheetAssetPath data, string expected)
        {
            var converter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
            var context = new SheetValueConvertingContext(converter, SheetContractResolver.Instance);
            var value = context.ValueToString(data.GetType(), data);

            Assert.Equal(expected, value);
        }

        [Theory]
        [MemberData(nameof(AssetPathValueToStringTestData))]
        public void TestValueToStringJson(ISheetAssetPath data, string expected)
        {
            if (expected == null)
                expected = "null";
            else
                expected = $"\"{expected}\"";

            var value = JsonConvert.SerializeObject(
                data, new JsonSheetAssetPathConverter());

            Assert.Equal(expected, value);
        }
    }
}
