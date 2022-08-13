using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples.Google
{
    public class ItemSheet : Sheet<ItemSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; set; }
            public int Price { get; set; }
        }
    }

    public enum StatType { Hp, Attack, Defense, Speed }

    public class CharacterSheet : Sheet<CharacterSheet.Row>
    {
        public class Row : SheetRowArray<Elem>
        {
            public string Name { get; set; }
        }

        public class Elem : SheetRowElem
        {
            public int Level { get; set; }
            public ItemSheet.Reference RequiredItem { get; set; }
            public Dictionary<StatType, int> Stat { get; set; }
        }
    }

    public class SheetContainer : SheetContainerBase
    {
        public SheetContainer(Microsoft.Extensions.Logging.ILogger logger) : base(logger)
        { }

        public ItemSheet Simple { get; set; }
        public CharacterSheet Complex { get; set; }
    }
}