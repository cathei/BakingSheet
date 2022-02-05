using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cathei.BakingSheet.Examples
{
    public class SheetContainer : SheetContainerBase
    {
        public SheetContainer(ILogger logger) : base(logger) {}

        // use name of each matching sheet name from source
        public ConstantSheet Constants { get; private set; }
        public HeroSheet Heroes { get; private set; }
        public ItemSheet Items { get; private set; }
        public MonsterSheet Monsters { get; private set; }
        public DungeonSheet Dungeons { get; private set; }
        public NpcSheet Npcs { get; private set; }
    }
}
