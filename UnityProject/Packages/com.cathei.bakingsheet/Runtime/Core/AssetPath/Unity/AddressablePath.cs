// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// AssetPath representing path to Unity's Addressable Assets
    /// </summary>
    public partial class AddressablePath : AssetPath
    {
        private string subAssetName;

        protected override void Parse()
        {
            var match = DirectAssetPath.PathRegex.Match(RawValue);

            if (!match.Success)
            {
                fullPath = string.Empty;
                subAssetName = string.Empty;
                return;
            }

            var pathGroup = match.Groups[0];
            var subAssetGroup = match.Groups[1];

            fullPath = Path.Combine(BasePath, $"{pathGroup.Value}{Extension}");
            subAssetName = subAssetGroup.Value;

            if (!string.IsNullOrEmpty(subAssetName))
                fullPath += $"[{subAssetName}]";
        }

        public string SubAssetName
        {
            get
            {
                if (!this.IsValid())
                    return null;

                if (fullPath == null)
                    Parse();

                return subAssetName;
            }
        }
    }
}
