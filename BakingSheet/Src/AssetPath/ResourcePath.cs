// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public partial class ResourcePath : ISheetAssetPath
    {
        [Preserve]
        public string FullPath { get; set; }

        public virtual string Prefix => string.Empty;
        public virtual string Postfix => string.Empty;
    }
}
