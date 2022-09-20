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

        UnityEngine.Object IUnitySheetAssetPath.Asset
        {
            get => _asset;
            set => _asset = value;
        }

        public UnityEngine.Object Load()
        {
            if (_asset != null)
                return _asset;

#if UNITY_EDITOR
            _asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(FullPath);
#endif

            return _asset;
        }
    }
}
