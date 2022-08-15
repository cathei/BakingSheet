using System;
using System.Collections;
using System.Collections.Generic;

namespace Cathei.BakingSheet
{
    public interface IVerticalList : IList { }

    public interface IVerticalList<T> : IVerticalList, IList<T>, IReadOnlyList<T> { }

    // same as List<T> but works vertical, like SheetRowArray
    public class VerticalList<T> : List<T>, IVerticalList<T>
    {
        public VerticalList() { }
        public VerticalList(IEnumerable<T> collection) : base(collection) { }
        public VerticalList(int capacity) : base(capacity) { }
    }
}
