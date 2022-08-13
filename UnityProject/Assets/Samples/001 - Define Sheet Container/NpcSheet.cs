using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public enum Situation
    {
        Greeting,
        Purchasing,
        Leaving
    }

    public struct SituationText
    {
        public string Greeting { get; private set; }
        public string Purchasing { get; private set; }
        public string Leaving { get; private set; }
    }

    public class NpcSheet : Sheet<NpcSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }

            public SituationText Texts { get; private set; }
        }
    }
}
