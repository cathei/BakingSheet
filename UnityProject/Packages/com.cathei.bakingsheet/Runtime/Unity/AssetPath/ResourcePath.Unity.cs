// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEngine;

namespace Cathei.BakingSheet
{
    public partial class ResourcePath
    {
        public override UnityEngine.Object Load()
        {
            if (Asset != null)
                return Asset;

            Asset = Resources.Load(FullPath);
            return Asset;
        }
    }
}
