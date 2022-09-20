// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEngine;

namespace Cathei.BakingSheet
{
    public partial class ResourcePath
    {
        private UnityEngine.Object _asset;

        public UnityEngine.Object Load()
        {
            if (_asset != null)
                return _asset;

            _asset = Resources.Load(FullPath);
            return _asset;
        }
    }
}
