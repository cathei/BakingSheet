using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class ItemSheet : Sheet<ItemSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }
            public int Price { get; private set; }
        }
    }
}
