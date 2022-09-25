// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEngine;

namespace Cathei.BakingSheet.Unity
{
    public partial class ResourcePath : IUnitySheetAssetPath
    {
        private UnityEngine.Object _asset;

        string IUnitySheetAssetPath.MetaType => SheetMetaType.ResourcePath;

        public T Load<T>() where T : UnityEngine.Object
        {
            if (_asset != null)
                return _asset as T;

            _asset = Resources.Load(FullPath);
            return _asset as T;
        }
    }
}
