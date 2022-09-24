// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cathei.BakingSheet
{
    public sealed class SheetContainerScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SheetScriptableObject> sheets;

        public IEnumerable<SheetScriptableObject> Sheets => sheets;

        private void Reset()
        {
            sheets = new List<SheetScriptableObject>();
        }

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
