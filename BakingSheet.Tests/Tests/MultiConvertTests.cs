using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class MultiConvertTests : IDisposable
    {
        private TestFileSystem _fileSystem;
        private TestLogger _logger;
        private TestSheetContainer _container;
        private CsvSheetConverter _csvConverter;
        private JsonSheetConverter _jsonConverter;

        public MultiConvertTests()
        {
            _fileSystem = new TestFileSystem();
            _logger = new TestLogger();
            _container = new TestSheetContainer(_logger);
            _csvConverter = new CsvSheetConverter("csvdata", TimeZoneInfo.Utc, fileSystem: _fileSystem);
            _jsonConverter = new JsonSheetConverter("jsondata", fileSystem: _fileSystem);
        }

        public void Dispose()
        {
            _fileSystem.Dispose();
        }

    }
}
