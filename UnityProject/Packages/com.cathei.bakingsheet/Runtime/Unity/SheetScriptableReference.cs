// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public partial class Sheet<TKey, TValue>
    {
        [Serializable]
        public struct ScriptableObject
        {
            [SerializeField]
            private SheetScriptableObject _asset;

            public TValue Row
            {
                get => _asset.GetRow<TValue>();
                set => _asset.SetRow(value);
            }
        }
    }
}
