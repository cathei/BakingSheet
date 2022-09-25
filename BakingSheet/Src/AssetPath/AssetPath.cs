// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public abstract class AssetPath : ISheetAssetPath
    {
        [Preserve]
        public string RelativePath { get; set; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;

        public virtual string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(RelativePath))
                    return null;

                return Path.Combine(BasePath, RelativePath + Extension);
            }
        }

        public virtual bool IsValid()
        {
            return !string.IsNullOrEmpty(RelativePath);
        }
    }
}
