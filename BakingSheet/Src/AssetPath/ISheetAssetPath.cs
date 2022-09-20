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
}
