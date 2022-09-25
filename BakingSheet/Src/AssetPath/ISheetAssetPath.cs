// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public interface ISheetAssetPath
    {
        string RawValue { get; }
        string FullPath { get; }
    }
}
