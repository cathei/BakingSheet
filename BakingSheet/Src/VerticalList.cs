// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Collections;
using System.Collections.Generic;

namespace Cathei.BakingSheet
{
    public interface IVerticalList { }

    /// <summary>
    /// Usage is same as generic List.
    /// When converting from/to sheet, index is vertical.
    /// SheetRowArray internally uses VerticalList.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class VerticalList<T> : List<T>, IVerticalList
    {
        public VerticalList() { }
        public VerticalList(IEnumerable<T> collection) : base(collection) { }
        public VerticalList(int capacity) : base(capacity) { }
    }
}
