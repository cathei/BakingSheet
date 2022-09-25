// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using System.Text.RegularExpressions;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// Direct asset reference to Unity's Assets folder
    /// Note that this is only supported when you are using ScriptableObject exporter in Unity
    /// </summary>
    public partial class DirectAssetPath : AssetPath
    {
        internal static readonly Regex PathRegex = new Regex(@"^([^\[\]]*)(?:\[([^\[\]]*)\])?$");

        private string subAssetName;

        protected override void Parse()
        {
            var match = PathRegex.Match(RawValue);

            if (!match.Success)
            {
                fullPath = string.Empty;
                subAssetName = string.Empty;
                return;
            }

            var pathGroup = match.Groups[0];
            var subAssetGroup = match.Groups[1];

            fullPath = Path.Combine(BasePath, pathGroup.Value + Extension);
            subAssetName = subAssetGroup.Value;
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
