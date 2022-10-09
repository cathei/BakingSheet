using System;
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
                var resultPath = Path.Combine(Application.streamingAssetsPath, "CSV");

                var sheetContainer = new SheetContainer();

                // create csv converter from path
                var csvImporter = new CsvSheetConverter(csvPath, TimeZoneInfo.Utc);

                // bake sheets from csv converter
                await sheetContainer.Bake(csvImporter);

                // create csv converter to path
                var csvExporter = new CsvSheetConverter(resultPath, TimeZoneInfo.Utc);

                // save datasheet to streaming assets
                await sheetContainer.Store(csvExporter);

                AssetDatabase.Refresh();

                Debug.Log("CSV sheet converted.");
            }
        }
    }
}
