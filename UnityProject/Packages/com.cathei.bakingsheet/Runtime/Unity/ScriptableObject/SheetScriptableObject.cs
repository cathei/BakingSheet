// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class SheetScriptableObject : ScriptableObject
    {
        // [SerializeField] private string _name;
        [SerializeField] private List<SheetRowScriptableObject> _rows;

        // public string Name => _name;
        public IEnumerable<SheetRowScriptableObject> Rows => _rows;
    }
}
