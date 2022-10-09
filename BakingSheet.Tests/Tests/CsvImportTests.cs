// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

            _logger.VerifyNoError();

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

            _logger.VerifyNoError();

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
            _fileSystem.SetTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn\nWrongEnum,1\nAlpha,2");

            var result = await _container.Bake(_converter);

            Assert.True(result);
            Assert.Single(_container.Types);

            _logger.VerifyLog(LogLevel.Error,
                "Failed to set id \"WrongEnum\"",
                new [] { "Types", "WrongEnum", "Id" });
        }

        [Fact]
        public async Task TestImportDuplicatedRow()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Types.csv"), "Id,IntColumn\nAlpha,2\nCharlie,3\nAlpha,4\nBravo,5");

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
            _fileSystem.SetTestData(Path.Combine("testdata", "Nested.csv"), "Id,Struct:XInt,Struct:YFloat,Struct:ZList:1,Struct:ZList:2,StructList:1:XInt,StructList:1:YFloat,StructList:1:ZList:1,StructList:1:ZList:2,StructList:2:XInt,StructList:2:YFloat,StructList:2:ZList:1,StructList:2:ZList:2,IntList:1,IntList:2,IntList:3,IntList:4,IntList:5\nRow1,0,0,,,,,,,,,,,1,2,3,,\n,,,,,,,,,,,,,4,5,6,7,8\nRow2,10,50.42,x,y,,,,,,,,\nRow3,0,0,,,1,0.124,a,b,2,20,c,,,,,,\n");

            var result = await _container.Bake(_converter);

            _logger.VerifyNoError();

            Assert.True(result);
            Assert.Equal(3, _container.Nested.Count);
            Assert.Equal(2, _container.Nested["Row1"].Count);
            Assert.Equal(3, _container.Nested["Row1"][0].IntList.Count);
            Assert.Equal(2, _container.Nested["Row3"].StructList.Count);
            Assert.Equal(10, _container.Nested["Row2"].Struct.XInt);
            Assert.Equal(1, _container.Nested["Row3"].StructList[0].XInt);
            Assert.Equal("b", _container.Nested["Row3"].StructList[0].ZList[1]);
            Assert.Null(_container.Nested["Row1"].StructList);
        }

        [Fact]
        public async Task TestImportDictCsv()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Dict.csv"), "Id,Dict:A,Dict:B,Dict:C,NestedDict:2034:1,NestedDict:2034:2,NestedDict:2034:3,Value\r\nDict1,10,20,,X,YYY,ZZZZZ,0\r\nDict2,,20,10\r\nEmpty,,,,,,,8\r\n,,,,,,,65\r\n");

            var result = await _container.Bake(_converter);

            _logger.VerifyNoError();

            Assert.True(result);
            Assert.Equal(3, _container.Dict.Count);
            Assert.Single(_container.Dict["Dict1"]);
            Assert.Empty(_container.Dict["Dict2"]);
            Assert.Equal(2, _container.Dict["Dict1"].Dict.Count);
            Assert.Equal(10.0f, _container.Dict["Dict2"].Dict["C"]);
            Assert.Equal(3, _container.Dict["Dict1"][0].NestedDict[2034].Count);
            Assert.Equal("YYY", _container.Dict["Dict1"][0].NestedDict[2034][1]);
            Assert.Null(_container.Dict["Empty"].Dict);
        }

        [Fact]
        public async Task TestImportDictCsvSplit()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Dict.csv"), "Id,Dict,,,NestedDict,,,Value\r\n,A,B,C,2034,,,,\r\n,,,,1,2,3,\r\nDict1,10,20,,X,YYY,ZZZZZ,0\r\nDict2,,20,10\r\nEmpty,,,,,,,8\r\n,,,,,,,65\r\n");

            var result = await _container.Bake(_converter);

            _logger.VerifyNoError();

            Assert.True(result);
            Assert.Equal(3, _container.Dict.Count);
            Assert.Single(_container.Dict["Dict1"]);
            Assert.Empty(_container.Dict["Dict2"]);
            Assert.Equal(2, _container.Dict["Dict1"].Dict.Count);
            Assert.Equal(10.0f, _container.Dict["Dict2"].Dict["C"]);
            Assert.Equal(3, _container.Dict["Dict1"][0].NestedDict[2034].Count);
            Assert.Equal("YYY", _container.Dict["Dict1"][0].NestedDict[2034][1]);
            Assert.Null(_container.Dict["Empty"].Dict);
        }

        [Fact]
        public async Task TestImportVerticalCsv()
        {
            _fileSystem.SetTestData(Path.Combine("testdata", "Vertical.csv"), "Id,Coord:X,Coord:Y,Levels:1,Levels:2,Value\nVertical1,1,2,1,4,\n,2,3,2,5\n,,,3\nVertical2,1,2,,4,Elem1\n,,,,5,Elem2\n,,,,,Elem3\n");

            var result = await _container.Bake(_converter);

            _logger.VerifyNoError();

            Assert.Equal(2, _container.Vertical.Count);

            Assert.Empty(_container.Vertical["Vertical1"]);
            Assert.Equal(2, _container.Vertical["Vertical1"].Coord.Count);
            Assert.Equal(1, _container.Vertical["Vertical1"].Coord[0].X);
            Assert.Equal(3, _container.Vertical["Vertical1"].Coord[1].Y);

            Assert.Single(_container.Vertical["Vertical2"].Coord);
            Assert.Equal(2, _container.Vertical["Vertical2"].Levels.Count);
            Assert.Null(_container.Vertical["Vertical2"].Levels[0]);
            Assert.Equal(5, _container.Vertical["Vertical2"].Levels[1][1]);
            Assert.Equal("Elem3", _container.Vertical["Vertical2"][2].Value);
        }
    }
}