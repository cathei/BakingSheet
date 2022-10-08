// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ReferenceTests_Duplicated : IDisposable
    {
        public class DuplicatedRow : SheetRow { }

        public class DuplicatedSheet1 : Sheet<DuplicatedRow> { }

        public class DuplicatedSheet2 : Sheet<DuplicatedRow> { }

        public class DuplicatedSheetContainer : SheetContainerBase
        {
            public DuplicatedSheetContainer(ILogger logger) : base(logger) { }

            public DuplicatedSheet1 Sheet1 { get; set; }
            public DuplicatedSheet2 Sheet2 { get; set; }
        }

        private TestLogger _logger;
        private DuplicatedSheetContainer _container;

        public ReferenceTests_Duplicated()
        {
            _logger = new TestLogger();
            _container = new DuplicatedSheetContainer(_logger);
        }

        public void Dispose()
        {

        }

        [Fact]
        public void TestDuplicateRowError()
        {
            _container.Sheet1 = new DuplicatedSheet1();
            _container.Sheet2 = new DuplicatedSheet2();

            _container.PostLoad();

            _logger.VerifyLog(LogLevel.Error, "Duplicated Row type is used for Sheet2");
        }
    }
}
