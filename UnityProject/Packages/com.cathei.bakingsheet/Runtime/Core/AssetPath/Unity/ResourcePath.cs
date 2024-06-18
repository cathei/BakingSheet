// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Resource folder.
    /// User can specify sub asset name with square bracket "My/Asset/Path[SubAssetName]".
    /// </summary>
    public partial class ResourcePath : ISheetAssetPath
    {
        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        public virtual string BasePath => string.Empty;
        public virtual string DirectorySeparator => "/";

        public ResourcePath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            var match = DirectAssetPath.PathRegex.Match(RawValue);

            if (!match.Success)
                return;

            var filePath = match.Groups[1].Value;
            var subAssetName = match.Groups[2].Value;

            FullPath = AssetPath.CombinePath(BasePath, filePath, DirectorySeparator);
            SubAssetName = subAssetName;
        }
    }
}
