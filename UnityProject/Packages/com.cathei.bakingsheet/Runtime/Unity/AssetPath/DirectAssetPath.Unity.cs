// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet.Unity
{
    public interface IUnitySheetDirectAssetPath : IUnitySheetAssetPath
    {
        string SubAssetName { get; }
        UnityEngine.Object Asset { get; set; }
    }

    public partial class DirectAssetPath : IUnitySheetDirectAssetPath
    {
        private UnityEngine.Object _asset;

        string IUnitySheetAssetPath.MetaType => SheetMetaType.DirectAssetPath;

        UnityEngine.Object IUnitySheetDirectAssetPath.Asset
        {
            get => _asset;
            set => _asset = value;
        }

        public T Get<T>() where T : UnityEngine.Object
        {
            return _asset as T;
        }
    }
}
