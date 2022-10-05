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
            if (!this.IsValid())
                return null;

            if (_asset != null)
                return _asset as T;

            if (string.IsNullOrEmpty(SubAssetName))
            {
                _asset = Resources.Load(FullPath);
            }
            else
            {
                var candidates = Resources.LoadAll(FullPath);

                // skip the first asset (main asset)
                for (int i = 1; i < candidates.Length; ++i)
                {
                    if (candidates[i].name == SubAssetName)
                        _asset = candidates[i];
                }
            }

            return _asset as T;
        }
    }
}
