// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEngine;

namespace Cathei.BakingSheet
{
    public partial class ResourcePath
    {
        private UnityEngine.Object _asset;

        public T Load<T>() where T : UnityEngine.Object
        {
            if (_asset != null)
                return _asset as T;

            _asset = Resources.Load(FullPath);
            return _asset as T;
        }
    }
}
