using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public enum GameConstant
    {
        ServerAddress,
        InitialGold,
        CriticalChance,
    }

    public class ConstantSheet : Sheet<GameConstant, ConstantSheet.Row>
    {
        public class Row : SheetRow<GameConstant>
        {
            public string Value { get; private set; }

            private int valueInt;
            public int ValueInt => valueInt;

            private float valueFloat;
            public float ValueFloat => valueFloat;

            public override void PostLoad(SheetConvertingContext context)
            {
                base.PostLoad(context);

                int.TryParse(Value, out valueInt);
                float.TryParse(Value, out valueFloat);
            }
        }

        public string GetString(GameConstant key)
        {
            return Find(key).Value;
        }

        public int GetInt(GameConstant key)
        {
            return Find(key).ValueInt;
        }

        public float GetFloat(GameConstant key)
        {
            return Find(key).ValueFloat;
        }
    }
}
