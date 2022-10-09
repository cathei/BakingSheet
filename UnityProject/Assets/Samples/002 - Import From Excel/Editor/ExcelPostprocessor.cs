using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class ExcelPostprocessor : AssetPostprocessor
    {
        static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // automatically run postprocessor if any excel file is imported
            string excelAsset = importedAssets.FirstOrDefault(x => x.EndsWith(".xlsx"));

            if (excelAsset != null)
            {
                var excelPath = Path.GetDirectoryName(excelAsset);
                var jsonPath = Path.Combine(Application.streamingAssetsPath, "Excel");

                var sheetContainer = new SheetContainer();

                // create excel converter from path
                var excelConverter = new ExcelSheetConverter(excelPath, TimeZoneInfo.Utc);

                // bake sheets from excel converter
                await sheetContainer.Bake(excelConverter);

                // (optional) verify that data is correct
                sheetContainer.Verify(
#if BAKINGSHEET_ADDRESSABLES
                    new AddressablePathVerifier(),
#endif
                    new ResourcePathVerifier()
                );

                // create json converter to path
                var jsonConverter = new JsonSheetConverter(jsonPath);

                // save datasheet to streaming assets
                await sheetContainer.Store(jsonConverter);

                AssetDatabase.Refresh();

                Debug.Log("Excel sheet converted.");
            }
        }
    }
}
