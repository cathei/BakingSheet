// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public interface ISheetAssetPath
    {
        string FullPath { get; set; }

        string Prefix { get; }
        string Postfix { get; }
    }

    public partial class AssetPath : ISheetAssetPath
    {
        [Preserve]
        public string FullPath { get; set; }

        public virtual string Prefix => string.Empty;
        public virtual string Postfix => string.Empty;
    }
}
