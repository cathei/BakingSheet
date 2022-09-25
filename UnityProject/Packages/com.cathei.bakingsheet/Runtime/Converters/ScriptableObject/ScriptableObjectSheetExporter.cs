// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Cathei.BakingSheet.Unity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class ScriptableObjectSheetExporter : ISheetExporter, ISheetFormatter
    {
        private readonly string _savePath;

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.Utc;
        public IFormatProvider FormatProvider => CultureInfo.InvariantCulture;

        public ScriptableObjectSheetExporter(string path)
        {
            _savePath = path;
        }

        public Task<bool> Export(SheetConvertingContext context)
        {
            var rowToSO = new Dictionary<ISheetRow, SheetRowScriptableObject>();

            var containerSO = GenerateAssets(_savePath, context, this, rowToSO);

            RemapReferences(context, rowToSO);

            SaveAssets(containerSO);

            return Task.FromResult(true);
        }

        private static SheetContainerScriptableObject GenerateAssets(
            string savePath, SheetConvertingContext context, ISheetFormatter formatter,
            Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            var props = context.Container.GetSheetProperties();

            if (!AssetDatabase.IsValidFolder(savePath))
            {
                savePath = AssetDatabase.CreateFolder(
                    Path.GetDirectoryName(savePath), Path.GetFileName(savePath));
            }

            var valueContext = new SheetValueConvertingContext(formatter, new SheetContractResolver());

            string containerPath = Path.Combine(savePath, "_Container.asset");

            var containerSO = AssetDatabase.LoadAssetAtPath<SheetContainerScriptableObject>(containerPath);

            if (containerSO == null)
            {
                containerSO = ScriptableObject.CreateInstance<SheetContainerScriptableObject>();
                AssetDatabase.CreateAsset(containerSO, containerPath);
            }

            containerSO.Clear();

            var existingRowSO = new Dictionary<string, SheetRowScriptableObject>();

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

                    foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
                    {
                        if (!(asset is SheetRowScriptableObject rowSO))
                            continue;

                        string rowIdStr = rowSO.name;

                        existingRowSO.Add(rowIdStr, rowSO);
                    }

                    sheetSO.Clear();

                    foreach (var row in sheet)
                    {
                        string rowIdStr = valueContext.ValueToString(row.Id.GetType(), row.Id);

                        if (!existingRowSO.TryGetValue(rowIdStr, out var rowSO))
                        {
                            rowSO = ScriptableObject.CreateInstance<JsonSheetRowScriptableObject>();
                            AssetDatabase.AddObjectToAsset(rowSO, sheetSO);
                        }

                        rowSO.name = rowIdStr;
                        rowSO.SetRow(row);

                        sheetSO.Add(rowSO);
                        rowToSO.Add(row, rowSO);
                        existingRowSO.Remove(rowIdStr);
                    }

                    // clear removed scriptable objects
                    foreach (var rowSO in existingRowSO.Values)
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
                        if (typeof(IUnitySheetReference).IsAssignableFrom(node.ValueType))
                        {
                            ProcessRemap(context, sheet, node, indexes, SheetReferenceMapping, rowToSO);
                        }
                        else if (typeof(IUnitySheetDirectAssetPath).IsAssignableFrom(node.ValueType))
                        {
                            ProcessRemap(context, sheet, node, indexes, AssetReferenceMapping, 0);
                        }
                    }
                }
            }
        }

        private static void ProcessRemap<TState>(SheetConvertingContext context, ISheet sheet,
            PropertyMap.Node node, IEnumerable<object> indexes,
            Action<SheetConvertingContext, object, TState> mapper, TState state)
        {
            foreach (var row in sheet)
            {
                int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                using (context.Logger.BeginScope(row.Id))
                using (context.Logger.BeginScope(node.FullPath, indexes))
                {
                    for (int vindex = 0; vindex < verticalCount; ++vindex)
                    {
                        var obj = node.GetValue(row, vindex, indexes.GetEnumerator());
                        mapper(context, obj, state);

                        // setting back for value type struct
                        if (node.ValueType.IsValueType)
                            node.SetValue(row, vindex, indexes.GetEnumerator(), obj);
                    }
                }
            }
        }

        private static void SheetReferenceMapping(
            SheetConvertingContext context, object obj, Dictionary<ISheetRow, SheetRowScriptableObject> rowToSO)
        {
            if (!(obj is IUnitySheetReference refer))
                return;

            if (!refer.IsValid())
                return;

            if (!rowToSO.TryGetValue(refer.Ref, out var asset))
            {
                context.Logger.LogError("Failed to find reference \"{ReferenceId}\" from Asset", refer.Id);
                return;
            }

            refer.Asset = asset;
        }

        private static void AssetReferenceMapping(SheetConvertingContext context, object obj, int _)
        {
            if (!(obj is IUnitySheetDirectAssetPath path))
                return;

            if (!path.IsValid())
                return;

            var fullPath = path.FullPath;
            var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(fullPath);

            if (asset == null)
            {
                context.Logger.LogError("Failed to find asset \"{AssetPath}\" from Asset", fullPath);
                return;
            }

            path.Asset = asset;
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
