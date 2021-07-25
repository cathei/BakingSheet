using System;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ImportUnitTests : IDisposable
    {
        private TestFileSystem _fileSystem;

        public ImportUnitTests()
        {
            _fileSystem = new TestFileSystem();
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

        [Fact]
        public void TestImportEmptyCsv()
        {
            _fileSystem.SetTestData("jsonpath/Test.csv", "");
        }

        [Fact]
        public void TestImportJson()
        {

        }
    }
}
