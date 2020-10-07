using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    public static class PackageGenerationTools
    {
        [MenuItem("Tools/Generate Package")]
        public static void GeneratePackage()
        {
            var version = Environment.GetEnvironmentVariable("PROJECT_VERSION");

            if (version != null)
                PlayerSettings.bundleVersion = version;

            AssetDatabase.ExportPackage(new string [] {
                "Assets/Plugins/BakingSheet"
            }, $"BakingSheet.{PlayerSettings.bundleVersion}.unitypackage", ExportPackageOptions.Recurse);

            Debug.Log("Generating Unity Package Completed");
        }
    }
}
