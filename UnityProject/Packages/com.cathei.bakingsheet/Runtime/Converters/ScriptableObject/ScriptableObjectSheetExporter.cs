// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if UNITY_EDITOR

using System;
using System.IO;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Cathei.BakingSheet
{
    public class ScriptableObjectSheetExporter : ISheetExporter
    {
        private readonly string _savePath;

        public ScriptableObjectSheetExporter(string path)
        {
            _savePath = path;
        }

        public Task<bool> Export(SheetConvertingContext context)
        {
            var props = context.Container.GetSheetProperties();

            AssetDatabase.CreateFolder(
                Path.GetDirectoryName(_savePath), Path.GetFileName(_savePath));

            var containerSO = ScriptableObject.CreateInstance<SheetContainerScriptableObject>();
            AssetDatabase.CreateAsset(containerSO, Path.Combine(_savePath, "_Container.asset"));

            foreach (var prop in props)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container) as ISheet;

                    if (sheet == null)
                        continue;

                    var sheetSO = ScriptableObject.CreateInstance<SheetScriptableObject>();
                    string sheetPath = Path.Combine(_savePath, $"{sheet.Name}.asset");

                    AssetDatabase.CreateAsset(sheetSO, sheetPath);

                    foreach (ISheetRow row in sheet)
                    {
                        var rowSO = ScriptableObject.CreateInstance<JsonSheetRowScriptableObject>();
                        rowSO.SetRow(row);

                        AssetDatabase.AddObjectToAsset(rowSO, sheetSO);
                    }

                    containerSO.Add(sheetSO);
                }
            }

            return Task.FromResult(true);
        }
    }
}

#endif
