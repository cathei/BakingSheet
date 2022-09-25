// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public abstract class AssetPath : ISheetAssetPath
    {
        [Preserve]
        public string RawValue { get; set; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;

        protected string fullPath;

        protected virtual void Parse()
        {
            fullPath = Path.Combine(BasePath, RawValue + Extension);
        }

        public virtual string FullPath
        {
            get
            {
                if (!this.IsValid())
                    return null;

                if (fullPath == null)
                    Parse();

                return fullPath;
            }
        }
    }

    public static class AssetPathExtensions
    {
        public static bool IsValid(this ISheetAssetPath assetPath)
        {
            return !string.IsNullOrEmpty(assetPath?.RawValue);
        }
    }
}
