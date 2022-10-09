using Cathei.BakingSheet.Unity;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SheetLoader : MonoBehaviour
    {
        public SheetContainer Sheet { get; private set; }

        private async void Start()
        {
#if BAKINGSHEET_BETTERSTREAMINGASSETS
            Debug.Log("Scene loaded. (BetterStreamingAssets)");

            // If you're using StreamingAssets from Android, StreamingAssetsFileSystem must be used
            // Path is relative to StreamingAssets folder
            var jsonConverter = new JsonSheetConverter("Excel", new StreamingAssetsFileSystem());
#else
            Debug.Log("Scene loaded.");

            var jsonConverter = new JsonSheetConverter($"{Application.streamingAssetsPath}/Excel");
#endif

            Sheet = new SheetContainer();
            await Sheet.Bake(jsonConverter);

            Debug.Log(Sheet.Constants.Count);
            Debug.Log(Sheet.Heroes["HERO001"].Count);
            Debug.Log(Sheet.Heroes["HERO001"].GetLevel(5).RequiredItem.Ref.Name);
            Debug.Log(Sheet.Items["ITEM_POTION001"].Name);
        }
    }
}
