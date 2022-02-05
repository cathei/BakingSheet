using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    public static class PackageGenerationTools
    {
        [MenuItem("Tools/Generate Package")]
        public static void GeneratePackage()
        {
            var githubRef = Environment.GetEnvironmentVariable("GITHUB_REF");

            // GITHUB_REF = refs/heads/v1.X.X
            if (githubRef != null)
                PlayerSettings.bundleVersion = githubRef.Substring(11);

            AssetDatabase.ExportPackage(new string[] {
                "Assets/Plugins/BakingSheet"
            }, GetPackagePath("BakingSheet"), ExportPackageOptions.Recurse);

            Debug.Log("Generating Unity Package Completed");
        }

        private static string GetPackagePath(string title)
        {
            return Path.Combine(
                Path.GetDirectoryName(Directory.GetCurrentDirectory()), "Build",
                $"{title}.{PlayerSettings.bundleVersion}.unitypackage");
        }
    }
}
