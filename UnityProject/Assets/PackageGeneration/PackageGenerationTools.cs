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
        const string PackagePath = "Packages/com.cathei.bakingsheet";
        const string SamplePath = "Assets/Samples";

        [MenuItem("Tools/Generate Package")]
        public static void GeneratePackage()
        {
            // GITHUB_REF = refs/heads/v1.X.X
            string githubRef = Environment.GetEnvironmentVariable("GITHUB_REF");
            string githubVersion = githubRef?.Substring(11);

            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

            if (githubVersion != null && info.version != githubVersion)
                throw new InvalidOperationException("Package version does not match GitHub ref");

            string savePath = GetPackagePath("BakingSheet", info.version);

            AssetDatabase.ExportPackage(
                new[] { PackagePath, }, savePath, ExportPackageOptions.Recurse);

            string sampleSavePath = GetPackagePath("BakingSheet.Samples", info.version);

            AssetDatabase.ExportPackage(
                new[] { SamplePath, }, sampleSavePath, ExportPackageOptions.Recurse);

            Debug.Log($"Generating Unity Package Completed: {savePath} {sampleSavePath}");
        }

        [MenuItem("Tools/Generate Package (AssetStore)")]
        public static void GeneratePackageCombined()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PackagePath);

            string savePath = GetPackagePath("BakingSheet.AssetStore", info.version);

            AssetDatabase.ExportPackage(
                new[] { PackagePath, SamplePath }, savePath, ExportPackageOptions.Recurse);

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
