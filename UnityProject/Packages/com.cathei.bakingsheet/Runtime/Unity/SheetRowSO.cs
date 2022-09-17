// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet.Internal
{
    public interface ISheetScriptableObjectReference
    {
        public SheetScriptableObject Asset { get; set; }
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
            private SheetScriptableObject _asset;

            public SheetScriptableObject Asset
            {
                get => _asset;
                set => _asset = value;
            }

            public TValue Row
            {
                get => _asset.GetRow<TValue>();
                set => _asset.SetRow(value);
            }
        }
    }
}
