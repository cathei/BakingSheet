// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Cathei.BakingSheet.Unity
{
    public class BakingSheetPluginImportCallback : IShouldIncludeInBuildCallback
    {
        public string PackageName { get; }

        public BakingSheetPluginImportCallback(string packageName)
        {
            PackageName = packageName;
        }

        private const string BakingSheetPackagePath = "com.cathei.bakingsheet";
        private const string CsvSubDirectory = "/Runtime/Converters/Csv";
        private const string GoogleSubDirectory = "/Runtime/Converters/Google";

        private const string CsvDefiningSymbol = "BAKINGSHEET_RUNTIME_CSVCONVERTER";
        private const string GoogleDefiningSymbol = "BAKINGSHEET_RUNTIME_GOOGLECONVERTER";

        [InitializeOnLoadMethod]
        private static void RegisterCallback()
        {
            // this is just failsafe register for when Unity finally fix their bug
            BuildUtilities.RegisterShouldIncludeInBuildCallback(
                new BakingSheetPluginImportCallback(BakingSheetPackagePath));

            // Bug workaround: Unity cannot even get correct PackageName, so we have to include subdirectory
            // https://forum.unity.com/threads/define-constraints-are-not-filtering-plugins-pluginimporter-defineconstraints-also-has-no-effect.1246426/

            BuildUtilities.RegisterShouldIncludeInBuildCallback(
                new BakingSheetPluginImportCallback(BakingSheetPackagePath + CsvSubDirectory));

            BuildUtilities.RegisterShouldIncludeInBuildCallback(
                new BakingSheetPluginImportCallback(BakingSheetPackagePath + GoogleSubDirectory));
        }

        public bool ShouldIncludeInBuild(string absolutePath)
        {
            // We will evaluate from build settings instead of using #if
            // Because it can give confusion when building via script
            // TODO: still cannot get extraScriptingDefines and there is no API for it

            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup, out var currentSymbols);

            // We have to check with Contains because Unity gives damn absolute path
            if (absolutePath.Contains(CsvSubDirectory))
                return currentSymbols.Contains(CsvDefiningSymbol);

            if (absolutePath.Contains(GoogleSubDirectory))
                return currentSymbols.Contains(GoogleDefiningSymbol);

            // fallback to default
            return true;
        }
    }
}
