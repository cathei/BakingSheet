// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public interface ISheetAssetPath
    {
        string RawValue { get; set; }
        string FullPath { get; }

        string BasePath { get; }
        string Extension { get; }
    }
}
