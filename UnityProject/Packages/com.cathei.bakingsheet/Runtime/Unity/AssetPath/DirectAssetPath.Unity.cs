// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public interface IUnitySheetDirectAssetPath : ISheetAssetPath
    {
        UnityEngine.Object Asset { get; set; }
    }

    public partial class DirectAssetPath : IUnitySheetDirectAssetPath
    {
        private UnityEngine.Object _asset;

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
