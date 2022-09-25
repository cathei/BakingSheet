// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Addressable Assets
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

            var pathGroup = match.Groups[1];
            var subAssetGroup = match.Groups[2];

            FullPath = Path.Combine(BasePath, $"{pathGroup.Value}{Extension}");
            SubAssetName = subAssetGroup.Value;

            if (!string.IsNullOrEmpty(SubAssetName))
                FullPath += $"[{SubAssetName}]";
        }
    }
}
