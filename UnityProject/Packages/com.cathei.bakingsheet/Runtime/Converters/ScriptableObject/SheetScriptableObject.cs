// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class SheetScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SheetRowScriptableObject> rows = new List<SheetRowScriptableObject>();

        public IEnumerable<SheetRowScriptableObject> Rows => rows;

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
