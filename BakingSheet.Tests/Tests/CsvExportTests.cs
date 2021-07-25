using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class CsvExportTests
    {
        private TestFileSystem _fileSystem;
        private Mock<ILogger> _loggerMock;
        private TestSheetContainer _container;
        private CsvSheetConverter _converter;

        public CsvExportTests()
        {
            _fileSystem = new TestFileSystem();
            _loggerMock = new Mock<ILogger>();
            _container = new TestSheetContainer(_loggerMock.Object);
            _converter = new CsvSheetConverter("testdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
        }
    }
}
