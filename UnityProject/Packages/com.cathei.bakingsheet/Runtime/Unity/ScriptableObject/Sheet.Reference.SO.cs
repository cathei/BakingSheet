// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet.Internal
{
    public interface ISheetScriptableObjectReference
    {
        public SheetRowScriptableObject Asset { get; set; }
    }
}

namespace Cathei.BakingSheet
{
    public partial class Sheet<TKey, TValue>
    {
        [Serializable]
        public struct RowSO : ISheetScriptableObjectReference
        {
            [SerializeField]
            private SheetRowScriptableObject asset;

            public SheetRowScriptableObject Asset
            {
                get => asset;
                set => asset = value;
            }

            public TValue Row
            {
                get => asset.GetRow<TValue>();
                set => asset.SetRow(value);
            }
        }
    }
}
