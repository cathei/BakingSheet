using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class DungeonSheet : Sheet<DungeonSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }

            public List<MonsterSheet.Reference> Monsters { get; private set; }
            public List<ItemSheet.Reference> Items { get; private set; }
        }
    }
}
