using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                var logger = new UnityLogger();
                var sheetContainer = new SheetContainer(logger);

                // create excel importer from path
                var excelImporter = new ExcelSheetImporter(excelPath);

                // bake sheets from excel importer
                await sheetContainer.Bake(excelImporter);

                // save datasheet to streaming assets
                await sheetContainer.Store(Path.Combine(Application.streamingAssetsPath, "Excel"));

                AssetDatabase.Refresh();

                Debug.Log("Excel sheet converted.");
            }
        }
    }
}
