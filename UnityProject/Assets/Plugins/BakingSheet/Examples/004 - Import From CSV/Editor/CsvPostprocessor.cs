using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class CsvPostprocessor : AssetPostprocessor
    {
        static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // automatically run postprocessor if any excel file is imported
            string csvAsset = importedAssets.FirstOrDefault(x => x.EndsWith(".csv"));

            if (csvAsset != null)
            {
                var csvPath = Path.GetDirectoryName(csvAsset);

                var logger = new UnityLogger();
                var sheetContainer = new SheetContainer(logger);

                // create excel importer from path
                var csvImporter = new CsvSheetImporter(csvPath);

                // bake sheets from excel importer
                await sheetContainer.Bake(csvImporter);

                // save datasheet to streaming assets
                await sheetContainer.Store(Path.Combine(Application.streamingAssetsPath, "CSV"));

                AssetDatabase.Refresh();

                Debug.Log("CSV sheet converted.");
            }
        }
    }
}
