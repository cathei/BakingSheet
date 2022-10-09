// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

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

        [InitializeOnLoadMethod]
        private static void RegisterCallback()
        {
            Debug.Log("RegisterCallback");

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
            Debug.Log("ShouldInclude: " + absolutePath);

            // TODO: is there any other way to evaluate defining symbol from script?
            // We have to check with Contains because Unity gives damn absolute path

            if (absolutePath.Contains(CsvSubDirectory))
            {
#if BAKINGSHEET_RUNTIME_CSVCONVERTER
                return true;
#endif
                return false;
            }

            if (absolutePath.Contains(GoogleSubDirectory))
            {
#if BAKINGSHEET_RUNTIME_GOOGLECONVERTER
                return true;
#endif
                return false;
            }

            // fallback to default
            return true;
        }
    }
}
