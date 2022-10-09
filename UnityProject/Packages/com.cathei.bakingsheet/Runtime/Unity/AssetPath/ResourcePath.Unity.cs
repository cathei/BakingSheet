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

            _asset = Load(FullPath, SubAssetName);
            return _asset as T;
        }

        internal static UnityEngine.Object Load(string fullPath, string subAssetName)
        {
            if (string.IsNullOrEmpty(subAssetName))
                return Resources.Load(fullPath);

            var candidates = Resources.LoadAll(fullPath);

            // skip the first asset (main asset)
            for (int i = 1; i < candidates.Length; ++i)
            {
                if (candidates[i].name == subAssetName)
                    return candidates[i];
            }

            return null;
        }
    }
}
