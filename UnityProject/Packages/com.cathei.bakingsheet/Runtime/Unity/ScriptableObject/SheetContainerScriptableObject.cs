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
        [SerializeField] private List<SheetScriptableObject> _sheets;

        public IEnumerable<SheetScriptableObject> Sheets => _sheets;

        public void Add(SheetScriptableObject sheet)
        {
            _sheets.Add(sheet);
        }
    }
}
