using System;
using Xunit;
using Cathei.BakingSheet.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Tests
{
    public class TestSheetContainer : SheetContainerBase
    {
        public TestSheetContainer(ILogger logger) : base(logger)
        {

        }
    }
}
