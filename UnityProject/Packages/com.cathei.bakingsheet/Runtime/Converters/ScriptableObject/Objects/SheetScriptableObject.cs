// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class SheetScriptableObject : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private string typeInfo;
#endif

        [SerializeField] private List<SheetRowScriptableObject> rows;

        public IEnumerable<SheetRowScriptableObject> Rows => rows;

        private void Reset()
        {
            rows = new List<SheetRowScriptableObject>();
        }

        internal Type Type
        {
            set => typeInfo = value.FullName;
        }

        internal void Clear()
        {
            rows.Clear();
        }

        internal void Add(SheetRowScriptableObject row)
        {
            rows.Add(row);
        }
    }
}
