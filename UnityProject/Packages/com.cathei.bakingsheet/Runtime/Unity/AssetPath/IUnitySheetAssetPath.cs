// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

namespace Cathei.BakingSheet.Unity
{
    public interface IUnitySheetAssetPath : ISheetAssetPath
    {
        string MetaType { get; }
        string SubAssetName { get; }
    }
}