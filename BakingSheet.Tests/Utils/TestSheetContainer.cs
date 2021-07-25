using System;
using Xunit;
using Cathei.BakingSheet.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Tests
{
    public class TestSheet : Sheet<TestSheet.Row>
    {
        public class Row : SheetRow
        {
            public string StringColumn { get; set; }
            public float FloatColumn { get; set; }
            public DateTime DateTimeColumn { get; set; }
            public TimeSpan TimeSpanColumn { get; set; }
        }
    }

    public class TestArraySheet : Sheet<TestArraySheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public TestSheet.Reference ReferColumn { get; set; }
            public TestArraySheet.Reference SelfReferColumn { get; set; }
            public int IntColumn { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {

        }
    }

    public class TestSheetContainer : SheetContainerBase
    {
        public TestSheetContainer(ILogger logger) : base(logger)
        {

        }

        public TestSheet Tests { get; private set; }
        public TestArraySheet TestArrays { get; private set; }
    }
}
