using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class CsvExportTests
    {
        private TestFileSystem _fileSystem;
        private TestLogger _logger;
        private TestSheetContainer _container;
        private CsvSheetConverter _converter;

        public CsvExportTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
            _converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }

        [Fact]
        public async Task TestExportEmptyCsv()
        {
            _container.Tests = new TestSheet();
            _container.Arrays = new TestArraySheet();

            _container.PostLoad();

            var result = await _container.Store(_converter);

            Assert.True(result);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Tests.csv"), "Id,Content\n");
            _fileSystem.VerifyTestData(Path.Combine("testdata", "Arrays.csv"), "Id,Content,ElemContent\n");
        }

        [Fact]
        public async Task TestExportSampleCsv()
        {
            _container.Tests = new TestSheet();
            
            var testRow = new TestSheet.Row {
                Id = "TestId",
                Content = "TestContent"
            };

            _container.Tests.Add(testRow);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Tests.csv"), "Id,Content\nTestId,TestContent\n");
        }

        [Fact]
        public async Task TestExportArrayCsv()
        {
            _container.Arrays = new TestArraySheet();

            var arrayRow = new TestArraySheet.Row {
                Id = "TestId",
                Content = "TestContent"
            };

            var arrayElem1 = new TestArraySheet.Elem {
                ElemContent = "TestElemContent1"
            };

            var arrayElem2 = new TestArraySheet.Elem {
                ElemContent = "TestElemContent2"
            };

            arrayRow.Arr.Add(arrayElem1);
            arrayRow.Arr.Add(arrayElem2);

            _container.Arrays.Add(arrayRow);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Arrays.csv"), "Id,Content,ElemContent\nTestId,TestContent,TestElemContent1\n,,TestElemContent2\n");
        }

        [Fact]
        public async Task TestExportNestedCsv()
        {
            _container.Nested = new TestNestedSheet();

            var row1 = new TestNestedSheet.Row {
                Id = "Row1",
                Struct = new List<TestNestedSheet.NestedStruct>()
            };

            var row2 = new TestNestedSheet.Row {
                Id = "Row2",
                Struct = null
            };

            var row3 = new TestNestedSheet.Row {
                Id = "Row3",
                Struct = new List<TestNestedSheet.NestedStruct> {
                    new TestNestedSheet.NestedStruct {
                        X = 1, Y = 10, Z = new[] { "a", "b" }
                    },
                    new TestNestedSheet.NestedStruct {
                        X = 2, Y = 20, Z = new[] { "c" }
                    }
                }
            };

            var elem1 = new TestNestedSheet.Elem {
                IntList = new List<int> { 1, 2, 3 }
            };

            var elem2 = new TestNestedSheet.Elem {
                IntList = new List<int> { 4, 5, 6, 7, 8 }
            };

            var elem3 = new TestNestedSheet.Elem {
                IntList = null
            };

            row1.Arr.Add(elem1);
            row1.Arr.Add(elem2);

            row3.Arr.Add(elem3);

            _container.Nested.Add(row1);
            _container.Nested.Add(row2);
            _container.Nested.Add(row3);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Nested.csv"), "Id,Struct.0.X,Struct.0.Y,Struct.0.Z.0,Struct.0.Z.1,Struct.1.X,Struct.1.Y,Struct.1.Z.0,Struct.1.Z.1,IntList.0,IntList.1,IntList.2,IntList.3,IntList.4\nRow1,,,,,,,,,1,2,3,,\n,,,,,,,,,4,5,6,7,8\nRow2,,,,,,,,\nRow3,1,10,a,b,2,20,c,,,,,,\n");
        }
    }
}
