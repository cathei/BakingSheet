// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cathei.BakingSheet
{
    public class SheetContainerScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SheetScriptableObject> sheets = new List<SheetScriptableObject>();

        public IEnumerable<SheetScriptableObject> Sheets => sheets;

        internal void Clear()
        {
            sheets.Clear();
        }

        internal void Add(SheetScriptableObject sheet)
        {
            sheets.Add(sheet);
        }
    }
}
