// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public interface IUnitySheetAssetPath : ISheetAssetPath
    {
        UnityEngine.Object Asset { get; set; }

        UnityEngine.Object Load();
    }

    public partial class AssetPath : IUnitySheetAssetPath
    {
        private UnityEngine.Object _asset;

        protected UnityEngine.Object Asset
        {
            get => _asset;
            set => _asset = value;
        }

        UnityEngine.Object IUnitySheetAssetPath.Asset
        {
            get => _asset;
            set => _asset = value;
        }

        public virtual UnityEngine.Object Load()
        {
            if (Asset != null)
                return Asset;

#if UNITY_EDITOR
            Asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(FullPath);
#endif

            return Asset;
        }
    }
}
