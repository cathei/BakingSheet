// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Addressable Assets.
    /// User can specify sub asset name with square bracket "My/Asset/Path.png[SubAssetName]".
    /// </summary>
    public partial class AddressablePath : ISheetAssetPath
    {
        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;

        public AddressablePath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            var match = DirectAssetPath.PathRegex.Match(RawValue);

            if (!match.Success)
                return;

            var filePath = match.Groups[1].Value;
            var subAssetName = match.Groups[2].Value;

            if (!string.IsNullOrEmpty(Extension))
                filePath = $"{filePath}.{Extension}";

            FullPath = Path.Combine(BasePath, filePath);
            SubAssetName = subAssetName;

            if (!string.IsNullOrEmpty(SubAssetName))
                FullPath += $"[{SubAssetName}]";
        }
    }
}
