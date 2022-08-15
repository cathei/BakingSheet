using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class SheetLoader : MonoBehaviour
    {
        public SheetContainer Sheet { get; private set; }

        private async void Start()
        {
            Debug.Log("Scene loaded.");

            // If you're using StreamingAssets from Android, StreamingAssetsFileSystem must be used
            // Path is relative to StreamingAssets folder
            var jsonConverter = new JsonSheetConverter("Excel", new StreamingAssetsFileSystem());

            Sheet = new SheetContainer(new UnityLogger());
            await Sheet.Bake(jsonConverter);

            Debug.Log(Sheet.Constants.Count);
            Debug.Log(Sheet.Heroes["HERO001"].Count);
            Debug.Log(Sheet.Heroes["HERO001"].GetLevel(5).RequiredItem.Ref.Name);
            Debug.Log(Sheet.Items["ITEM_POTION001"].Name);
        }
    }
}