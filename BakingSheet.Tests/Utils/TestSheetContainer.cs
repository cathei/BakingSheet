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
            public string Content { get; set; }
        }
    }

    public class TestArraySheet : Sheet<TestArraySheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public string ElemContent { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public string Content { get; set; }
        }
    }

    public enum TestEnum
    {
        Alpha, Bravo, Charlie
    }

    public class TestTypeSheet : Sheet<TestTypeSheet.Row>
    {
        public class Row : SheetRow
        {
            public int IntColumn { get; set; }
            public float FloatColumn { get; set; }
            public DateTime DateTimeColumn { get; set; }
            public TimeSpan TimeSpanColumn { get; set; }
            public TestEnum EnumColumn { get; set; }
        }
    }

    public class TestReferenceSheet : Sheet<TestReferenceSheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public TestSheet.Reference NestedReferColumn { get; set; }
            public TestArraySheet.Reference NestedSelfReferColumn { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public TestSheet.Reference ReferColumn { get; set; }
            public TestArraySheet.Reference SelfReferColumn { get; set; }
        }
    }

    public class TestSheetContainer : SheetContainerBase
    {
        public TestSheetContainer(ILogger logger) : base(logger)
        {

        }

        public TestSheet Tests { get; set; }
        public TestArraySheet Arrays { get; set; }
        public TestTypeSheet Types { get; set; }
        public TestReferenceSheet Refers { get; set; }
    }
}
