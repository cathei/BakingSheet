using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class MonsterSheet : Sheet<MonsterSheet.Row>
    {
        public class Row : SheetRowArray<Elem>
        {
            public string Name { get; private set; }
            public int Damage { get; private set; }
            public int DropGold { get; private set; }

            public DateTime? SpawnEventStart { get; private set; }
            public DateTime? SpawnEventEnd { get; private set; }
        }

        public class Elem : SheetRowElem
        {
            public ItemSheet.Reference DropItem { get; private set; }
            public float DropItemProb { get; private set; }
        }
    }
}
