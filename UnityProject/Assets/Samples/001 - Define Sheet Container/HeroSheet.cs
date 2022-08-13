using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class HeroSheet : Sheet<HeroSheet.Row>
    {
        public class Row : SheetRowArray<Elem>
        {
            public string Name { get; private set; }

            public int Strength { get; private set; }
            public int Inteligence { get; private set; }
            public int Vitality { get; private set; }

            public Elem GetLevel(int level)
            {
                return this[level - 1];
            }
        }

        public class Elem : SheetRowElem
        {
            public float StatMultiplier { get; private set; }
            public int RequiredExp { get; private set; }
            public ItemSheet.Reference RequiredItem { get; private set; }
        }
    }
}
