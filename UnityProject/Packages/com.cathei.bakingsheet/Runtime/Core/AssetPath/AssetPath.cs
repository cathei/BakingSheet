// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Generic ISheetAssetPath implementation
    /// All derived class must have constructor with single string parameter
    /// </summary>
    public class AssetPath : ISheetAssetPath
    {
        public string RawValue { get; }
        public string FullPath { get; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;

        [Preserve]
        public AssetPath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            FullPath = Path.Combine(BasePath, RawValue + Extension);
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
