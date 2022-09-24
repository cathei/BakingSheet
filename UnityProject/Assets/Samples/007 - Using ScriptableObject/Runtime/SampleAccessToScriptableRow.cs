using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SampleAccessToScriptableRow : MonoBehaviour
    {
        public HeroSheet.Reference hero;
        public ConstantSheet.Reference constant;
        public List<DungeonSheet.Reference> dungeons;

        public void Awake()
        {
            Debug.Log(hero.Ref.Name);
            Debug.Log(constant.Ref.ValueInt);

            foreach (var dungeon in dungeons)
            {
                foreach (var item in dungeon.Ref.Items)
                    Debug.Log(item.Ref.Name);
            }
        }
    }
}
