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
    public partial class DirectAssetPath : ISheetAssetPath
    {
        internal static readonly Regex PathRegex = new Regex(@"^([^\[\]]+)(?:\[([^\[\]]+)\])?$");

        public string RawValue { get; }
        public string FullPath { get; }
        public string SubAssetName { get; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;

        public DirectAssetPath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            var match = PathRegex.Match(RawValue);

            if (!match.Success)
                return;

            var pathGroup = match.Groups[1];
            var subAssetGroup = match.Groups[2];

            FullPath = Path.Combine(BasePath, pathGroup.Value + Extension);
            SubAssetName = subAssetGroup.Value;
        }
    }
}
