// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            var rowToSO = new Dictionary<ISheetRow, SheetRowScriptableObject>();

            var containerSO = GenerateAssets(_savePath, context, rowToSO);

            RemapReferences(context, rowToSO);

            SaveAssets(containerSO);

            return Task.FromResult(true);
        }

        private static SheetContainerScriptableObject GenerateAssets(
            string savePath, SheetConvertingContext context, Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            var props = context.Container.GetSheetProperties();

            if (!AssetDatabase.IsValidFolder(savePath))
            {
                savePath = AssetDatabase.CreateFolder(
                    Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
            }

            string containerPath = Path.Combine(savePath, "_Container.asset");

            var containerSO = AssetDatabase.LoadAssetAtPath<SheetContainerScriptableObject>(containerPath);

            if (containerSO == null)
            {
                containerSO = ScriptableObject.CreateInstance<SheetContainerScriptableObject>();
                AssetDatabase.CreateAsset(containerSO, containerPath);
            }

            containerSO.Clear();

            var existingRowSO = new Dictionary<object, SheetRowScriptableObject>();
            var assetToDelete = new List<SheetRowScriptableObject>();

            foreach (var prop in props)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container) as ISheet;

                    if (sheet == null)
                        continue;

                    string sheetPath = Path.Combine(savePath, $"{sheet.Name}.asset");

                    var sheetSO = AssetDatabase.LoadAssetAtPath<SheetScriptableObject>(sheetPath);

                    if (sheetSO == null)
                    {
                        sheetSO = ScriptableObject.CreateInstance<SheetScriptableObject>();
                        AssetDatabase.CreateAsset(sheetSO, sheetPath);
                    }

                    sheetSO.name = sheet.Name;
                    sheetSO.typeInfo = sheet.RowType.FullName;

                    existingRowSO.Clear();
                    assetToDelete.Clear();

                    foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
                    {
                        if (!(asset is SheetRowScriptableObject rowSO))
                            continue;

                        object rowId = rowSO.GetRow(sheet.RowType)?.Id;

                        if (rowId == null || !sheet.Contains(rowId))
                        {
                            assetToDelete.Add(rowSO);
                            continue;
                        }

                        existingRowSO.Add(rowId, rowSO);
                    }

                    sheetSO.Clear();

                    foreach (var row in sheet)
                    {
                        if (!existingRowSO.TryGetValue(row.Id, out var rowSO))
                        {
                            rowSO = ScriptableObject.CreateInstance<JsonSheetRowScriptableObject>();
                            AssetDatabase.AddObjectToAsset(rowSO, sheetSO);
                        }

                        rowSO.name = row.Id.ToString();
                        rowSO.SetRow(row);

                        sheetSO.Add(rowSO);
                        rowToSO.Add(row, rowSO);
                    }

                    // clear removed scriptable objects
                    foreach (var rowSO in assetToDelete)
                        AssetDatabase.RemoveObjectFromAsset(rowSO);

                    containerSO.Add(sheetSO);
                }
            }

            return containerSO;
        }

        private static void RemapReferences(
            SheetConvertingContext context, Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            var props = context.Container.GetSheetProperties();

            foreach (var prop in props)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var sheet = prop.GetValue(context.Container) as ISheet;

                    if (sheet == null)
                        continue;

                    var propertyMap = sheet.GetPropertyMap(context);

                    foreach (var (node, indexes) in propertyMap.TraverseLeaf())
                    {
                        if (!typeof(ISheetReference).IsAssignableFrom(node.ValueType))
                            continue;

                        foreach (var row in sheet)
                        {
                            int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                            using (context.Logger.BeginScope(row.Id))
                            using (context.Logger.BeginScope(node.FullPath, indexes))
                            {
                                for (int vindex = 0; vindex < verticalCount; ++vindex)
                                {
                                    var obj = node.GetValue(row, vindex, indexes.GetEnumerator());

                                    if (!(obj is IUnitySheetReference refer))
                                        continue;

                                    if (refer.Ref == null)
                                        continue;

                                    if (!rowToSO.TryGetValue(refer.Ref, out var asset))
                                    {
                                        context.Logger.LogError("Failed to find reference \"{ReferenceId}\" from Asset", refer.Id);
                                        continue;
                                    }

                                    refer.Asset = asset;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SaveAssets(SheetContainerScriptableObject containerSO)
        {
            EditorUtility.SetDirty(containerSO);

            foreach (var sheetSO in containerSO.Sheets)
            {
                EditorUtility.SetDirty(sheetSO);

                foreach (var rowSO in sheetSO.Rows)
                    EditorUtility.SetDirty(rowSO);
            }

            AssetDatabase.SaveAssets();
        }
    }
}

#endif
