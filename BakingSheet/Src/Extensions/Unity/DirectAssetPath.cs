// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.IO;
using System.Text.RegularExpressions;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// Direct asset reference to Unity's Assets folder.
    /// Note that this is only supported when you are using ScriptableObject exporter in Unity.
    /// User can specify sub asset name with square bracket "My/Asset/Path.png[SubAssetName]".
    /// </summary>
    public partial class DirectAssetPath : ISheetAssetPath
    {
        internal static readonly Regex PathRegex = new Regex(@"^([^\[\]]+)(?:\[([^\[\]]+)\])?$");

        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        public virtual string BasePath => "Assets";
        public virtual string Extension => string.Empty;

        public DirectAssetPath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            var match = PathRegex.Match(RawValue);

            if (!match.Success)
                return;

            var filePath = match.Groups[1].Value;
            var subAssetName = match.Groups[2].Value;

            if (!string.IsNullOrEmpty(Extension))
                filePath = $"{filePath}.{Extension}";

            FullPath = Path.Combine(BasePath, filePath);
            SubAssetName = subAssetName;
        }
    }
}
