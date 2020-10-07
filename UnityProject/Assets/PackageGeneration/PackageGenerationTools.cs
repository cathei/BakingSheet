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
            var releaseTag = Environment.GetEnvironmentVariable("RELEASE_TAG");

            // RELEASE_TAG = v1.X.X
            // remove 'v' from tag
            if (releaseTag != null)
                PlayerSettings.bundleVersion = releaseTag.Substring(1);

            AssetDatabase.ExportPackage(new string [] {
                "Assets/Plugins/BakingSheet"
            }, $"BakingSheet.{PlayerSettings.bundleVersion}.unitypackage", ExportPackageOptions.Recurse);

            Debug.Log("Generating Unity Package Completed");
        }
    }
}
