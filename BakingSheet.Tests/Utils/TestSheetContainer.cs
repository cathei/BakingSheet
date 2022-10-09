// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
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

    public class TestNestedSheet : Sheet<TestNestedSheet.Row>
    {
        public struct NestedStruct
        {
            public int XInt { get; set; }
            public float YFloat { get; set; }
            public List<string> ZList { get; set; }
        }

        public class Elem : SheetRowElem
        {
            public List<int> IntList { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public NestedStruct Struct { get; set; }
            public List<NestedStruct> StructList { get; set; }
        }
    }

    public enum TestEnum
    {
        Alpha, Bravo, Charlie
    }

    public class TestTypeSheet : Sheet<TestEnum, TestTypeSheet.Row>
    {
        public class Row : SheetRow<TestEnum>
        {
            [SheetValueConverter(typeof(MyIntConverter))]
            public int IntColumn { get; set; }
            public float FloatColumn { get; set; }
            public decimal DecimalColumn { get; set; }
            public DateTime DateTimeColumn { get; set; }
            public TimeSpan TimeSpanColumn { get; set; }
            public TestEnum? EnumColumn { get; set; }
        }
    }

    public class MyIntConverter : SheetValueConverter<int>
    {
        protected override int StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            return int.Parse(value) - 1;
        }

        protected override string ValueToString(Type type, int value, SheetValueConvertingContext context)
        {
            return (value + 1).ToString();
        }
    }

    public class TestReferenceSheet : Sheet<TestReferenceSheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public TestSheet.Reference NestedReferColumn { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public TestSheet.Reference ReferColumn { get; set; }
            public TestReferenceSheet.Reference SelfReferColumn { get; set; }
            public List<TestSheet.Reference> ReferList { get; set; }
        }
    }

    public class TestDictSheet : Sheet<TestDictSheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public Dictionary<int, List<string>> NestedDict { get; set; }
            public int Value { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public Dictionary<string, float> Dict { get; set; }
        }
    }

    public class TestVerticalSheet : Sheet<TestVerticalSheet.Row>
    {
        public class Elem : SheetRowElem
        {
            public string Value { get; set; }
        }

        public struct NestedStruct
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Row : SheetRowArray<Elem>
        {
            public VerticalList<NestedStruct> Coord { get; set; }
            public List<VerticalList<int>> Levels { get; set; }
        }
    }

    public class TestSheetContainer : SheetContainerBase
    {
        public TestSheetContainer(ILogger logger) : base(logger) { }

        public TestSheet Tests { get; set; }
        public TestArraySheet Arrays { get; set; }
        public TestTypeSheet Types { get; set; }
        public TestReferenceSheet Refers { get; set; }
        public TestNestedSheet Nested { get; set; }
        public TestDictSheet Dict { get; set; }
        public TestVerticalSheet Vertical { get; set; }
    }
}
