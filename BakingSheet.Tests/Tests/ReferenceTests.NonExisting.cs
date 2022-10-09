// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Cathei.BakingSheet.Tests
{
    public class ReferenceTests_NonExisting : IDisposable
    {
        public class NonExistingSheet : Sheet<NonExistingSheet.Row>
        {
            public class Row : SheetRow
            {
                public string Content { get; set; }
            }
        }

        public class NonExistingReferSheet : Sheet<NonExistingReferSheet.Row>
        {
            public class Row : SheetRow
            {
                public NonExistingSheet.Reference Refer { get; set; }
            }
        }

        public class NonExistingTestSheetContainer : SheetContainerBase
        {
            public NonExistingTestSheetContainer(ILogger logger) : base(logger) { }

            // public NonExistingSheet Sheet { get; set; }
            public NonExistingReferSheet ReferSheet { get; set; }
        }

        private TestLogger _logger;
        private NonExistingTestSheetContainer _container;

        public ReferenceTests_NonExisting()
        {
            _logger = new TestLogger();
            _container = new NonExistingTestSheetContainer(_logger);
        }

        public void Dispose()
        {

        }

        [Fact]
        public void TestNotExistingSheetReference()
        {
            _container.ReferSheet = new NonExistingReferSheet();

            _container.ReferSheet.Add(new NonExistingReferSheet.Row
            {
                Refer = new NonExistingSheet.Reference("WhereIsIt")
            });

            _container.PostLoad();

            _logger.VerifyLog(LogLevel.Error, "Failed to find sheet for Cathei.BakingSheet.Tests.ReferenceTests_NonExisting+NonExistingSheet+Row reference");
        }
    }
}
