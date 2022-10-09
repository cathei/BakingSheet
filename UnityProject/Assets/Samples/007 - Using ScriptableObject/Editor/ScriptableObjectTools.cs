using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public static class ScriptableObjectTools
    {

        [MenuItem("BakingSheet/Sample/Export To ScriptableObject")]
        static async void ExportToScriptableObject()
        {
            var sheetContainer = new SheetContainer();

            var jsonPath = Path.Combine(Application.streamingAssetsPath, "Excel");
            var importer = new JsonSheetConverter(jsonPath);

            await sheetContainer.Bake(importer);

            var exporter = new ScriptableObjectSheetExporter("Assets/Samples/Extras/ScriptableObject");

            await sheetContainer.Store(exporter);

            Debug.Log("Sheet exporting to ScriptableObject completed.", exporter.Result);
        }
    }
}
