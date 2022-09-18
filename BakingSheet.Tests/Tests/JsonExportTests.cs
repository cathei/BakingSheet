// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class JsonExportTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private TestLogger _logger;
        private TestSheetContainer _container;
        private JsonSheetConverter _converter;

        public JsonExportTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
            _converter = new JsonSheetConverter("testdata", fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public async Task TestExportReferenceJson()
        {
            _container.Tests = new TestSheet();
            _container.Refers = new TestReferenceSheet();

            var referRow = new TestReferenceSheet.Row();

            referRow.Id = "Refer";
            referRow.ReferColumn = new TestSheet.Reference("Test");
            referRow.ReferList = new List<TestSheet.Reference> { new TestSheet.Reference("Test"), new TestSheet.Reference("Test") };

            _container.Refers.Add(referRow);

            var testRow = new TestSheet.Row();

            testRow.Id = "Test";

            _container.Tests.Add(testRow);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _logger.VerifyNoError();

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Refers.json"), "[{\"ReferColumn\":\"Test\",\"SelfReferColumn\":null,\"ReferList\":[\"Test\",\"Test\"],\"Arr\":[],\"Id\":\"Refer\"}]");
        }

        [Fact]
        public async Task TestExportNestedJson()
        {
            _container.Nested = new TestNestedSheet();

            var row1 = new TestNestedSheet.Row {
                Id = "Row1",
                StructList = new List<TestNestedSheet.NestedStruct>()
            };

            var row2 = new TestNestedSheet.Row {
                Id = "Row2",
                Struct = new TestNestedSheet.NestedStruct {
                    XInt = 10, YFloat = 50.42f, ZList = new List<string> { "x", "y" }
                },
                StructList = null
            };

            var row3 = new TestNestedSheet.Row {
                Id = "Row3",
                StructList = new List<TestNestedSheet.NestedStruct> {
                    new TestNestedSheet.NestedStruct {
                        XInt = 1, YFloat = 0.124f, ZList = new List<string> { "a", "b" }
                    },
                    new TestNestedSheet.NestedStruct {
                        XInt = 2, YFloat = 20, ZList = new List<string> { "c" }
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

            _logger.VerifyNoError();

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Nested.json"), "[{\"Struct\":{\"XInt\":0,\"YFloat\":0.0,\"ZList\":null},\"StructList\":[],\"Arr\":[{\"IntList\":[1,2,3]},{\"IntList\":[4,5,6,7,8]}],\"Id\":\"Row1\"},{\"Struct\":{\"XInt\":10,\"YFloat\":50.42,\"ZList\":[\"x\",\"y\"]},\"StructList\":null,\"Arr\":[],\"Id\":\"Row2\"},{\"Struct\":{\"XInt\":0,\"YFloat\":0.0,\"ZList\":null},\"StructList\":[{\"XInt\":1,\"YFloat\":0.124,\"ZList\":[\"a\",\"b\"]},{\"XInt\":2,\"YFloat\":20.0,\"ZList\":[\"c\"]}],\"Arr\":[{\"IntList\":null}],\"Id\":\"Row3\"}]");
        }

        [Fact]
        public async Task TestExportDictJson()
        {
            _container.Dict = new TestDictSheet();

            var row1 = new TestDictSheet.Row {
                Id = "Dict1",
                Dict = new Dictionary<string, float> {
                    { "A", 10.0f }, { "B", 20.0f }
                }
            };

            var row2 = new TestDictSheet.Row {
                Id = "Dict2",
                Dict = new Dictionary<string, float> {
                    { "C", 10.0f }, { "B", 20.0f }
                }
            };

            var row3 = new TestDictSheet.Row {
                Id = "Empty",
            };

            var elem1 = new TestDictSheet.Elem {
                NestedDict = new Dictionary<int, List<string>> {
                    { 2034, new List<string> { "X", "YYY", "ZZZZZ" } }
                }
            };

            var elem2 = new TestDictSheet.Elem {
                Value = 8
            };

            var elem3 = new TestDictSheet.Elem {
                Value = 65
            };

            row1.Arr.Add(elem1);

            row3.Arr.Add(elem2);
            row3.Arr.Add(elem3);

            _container.Dict.Add(row1);
            _container.Dict.Add(row2);
            _container.Dict.Add(row3);

            _container.PostLoad();

            var result = await _container.Store(_converter);

            _logger.VerifyNoError();

            _fileSystem.VerifyTestData(Path.Combine("testdata", "Dict.json"), "[{\"Dict\":{\"A\":10.0,\"B\":20.0},\"Arr\":[{\"NestedDict\":{\"2034\":[\"X\",\"YYY\",\"ZZZZZ\"]},\"Value\":0}],\"Id\":\"Dict1\"},{\"Dict\":{\"C\":10.0,\"B\":20.0},\"Arr\":[],\"Id\":\"Dict2\"},{\"Dict\":null,\"Arr\":[{\"NestedDict\":null,\"Value\":8},{\"NestedDict\":null,\"Value\":65}],\"Id\":\"Empty\"}]");
        }
    }
}
