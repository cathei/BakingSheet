// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Unity
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
