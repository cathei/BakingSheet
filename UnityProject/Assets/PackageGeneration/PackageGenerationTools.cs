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
            // GITHUB_REF = refs/heads/v1.X.X
            string githubRef = Environment.GetEnvironmentVariable("GITHUB_REF");
            string githubVersion = githubRef?.Substring(11);

            string packagePath = "Packages/com.cathei.bakingsheet";

            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(packagePath);

            if (githubVersion != null && info.version != githubVersion)
                throw new InvalidOperationException("Package version does not match GitHub ref");

            string savePath = GetPackagePath("BakingSheet", info.version);

            AssetDatabase.ExportPackage(
                new[] { packagePath, }, savePath, ExportPackageOptions.Recurse);

            Debug.Log($"Generating Unity Package Completed: {savePath}");
        }

        private static string GetPackagePath(string title, string version)
        {
            return Path.Combine(
                Path.GetDirectoryName(Directory.GetCurrentDirectory()), "Build",
                $"{title}.{version}.unitypackage");
        }
    }
}
