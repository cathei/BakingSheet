// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if UNITY_EDITOR

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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

            if (!AssetDatabase.IsValidFolder(_savePath))
            {
                AssetDatabase.CreateFolder(
                    Path.GetDirectoryName(_savePath), Path.GetFileName(_savePath));
            }

            string containerPath = Path.Combine(_savePath, "_Container.asset");

            var containerSO = AssetDatabase.LoadAssetAtPath<SheetContainerScriptableObject>(containerPath);

            if (containerSO == null)
            {
                containerSO = ScriptableObject.CreateInstance<SheetContainerScriptableObject>();
                AssetDatabase.CreateAsset(containerSO, containerPath);
            }

            containerSO.Clear();

            foreach (var prop in props)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container) as ISheet;

                    if (sheet == null)
                        continue;

                    string sheetPath = Path.Combine(_savePath, $"{sheet.Name}.asset");

                    var sheetSO = AssetDatabase.LoadAssetAtPath<SheetScriptableObject>(sheetPath);

                    if (sheetSO == null)
                    {
                        sheetSO = ScriptableObject.CreateInstance<SheetScriptableObject>();
                        AssetDatabase.CreateAsset(sheetSO, sheetPath);
                    }

                    sheetSO.name = sheet.Name;

                    var rowSODict = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                        .OfType<SheetRowScriptableObject>()
                        .ToDictionary(x => x.GetRow(sheet.RowType).Id);

                    sheetSO.Clear();

                    foreach (ISheetRow row in sheet)
                    {
                        if (!rowSODict.TryGetValue(row.Id, out var rowSO))
                        {
                            rowSO = ScriptableObject.CreateInstance<JsonSheetRowScriptableObject>();
                            AssetDatabase.AddObjectToAsset(rowSO, sheetSO);
                        }

                        rowSO.name = row.Id.ToString();
                        rowSO.SetRow(row);

                        sheetSO.Add(rowSO);
                        rowSODict.Remove(row.Id);
                    }

                    // clear removed scriptable objects
                    foreach (var removedRowSO in rowSODict.Values)
                        AssetDatabase.RemoveObjectFromAsset(removedRowSO);

                    containerSO.Add(sheetSO);
                }
            }

            AssetDatabase.SaveAssets();

            return Task.FromResult(true);
        }
    }
}

#endif
