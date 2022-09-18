// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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
            public override string Prefix => "MyPath/";
            public override string Postfix => ".png";
        }

        public static IEnumerable<object[]> AssetPathStringToValueTestData()
        {
            yield return new object[] { typeof(AssetPath), "MyPng", "MyPng" };
            yield return new object[] { typeof(AssetPath), "123", "123" };
            yield return new object[] { typeof(AssetPath), "Nested/MyPng", "Nested/MyPng" };
            yield return new object[] { typeof(AssetPath), null, null };
            yield return new object[] { typeof(AssetPath), "", null };
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
            yield return new object[] { new AssetPath { FullPath = "MyPng" }, "MyPng" };
            yield return new object[] { new AssetPath { FullPath = "123" }, "123" };
            yield return new object[] { new AssetPath { FullPath = "Nested/MyPng" }, "Nested/MyPng" };
            yield return new object[] { new AssetPath { FullPath = null }, null };
            yield return new object[] { new AssetPath { FullPath = "" }, null };
            yield return new object[] { new TestPngAssetPath { FullPath = "MyPath/MyPng.png" }, "MyPng" };
            yield return new object[] { new TestPngAssetPath { FullPath = "MyPath/123.png" }, "123" };
            yield return new object[] { new TestPngAssetPath { FullPath = "MyPath/Nested/MyPng.png" }, "Nested/MyPng"};
            yield return new object[] { new TestPngAssetPath { FullPath = null }, null };
            yield return new object[] { new TestPngAssetPath { FullPath = "" }, null };
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
