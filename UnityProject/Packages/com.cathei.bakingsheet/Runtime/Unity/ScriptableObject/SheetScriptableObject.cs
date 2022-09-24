// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public sealed class SheetScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SheetRowScriptableObject> rows;

#if UNITY_EDITOR
        [SerializeField, HideInInspector] internal string typeInfo;
#endif

        public IEnumerable<SheetRowScriptableObject> Rows => rows;

        private void Reset()
        {
            rows = new List<SheetRowScriptableObject>();
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
