// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cathei.BakingSheet
{
    public partial interface ISheetAssetPath
    {
        public UnityEngine.Object Asset { get; set; }
    }

    public partial class AssetPath
    {
        private Object _asset;

        protected UnityEngine.Object Asset => _asset;

        UnityEngine.Object ISheetAssetPath.Asset
        {
            get => _asset;
            set => _asset = value;
        }
    }
}
