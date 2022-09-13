using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cathei.BakingSheet.Examples;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    public static class AssetMenuUtils
    {
        [MenuItem("Assets/Create/BakingSheet/Sheet SO")]
        public static void CreateScriptableObject()
        {
            var so = ScriptableObject.CreateInstance<SheetScriptableObject>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Sheet.asset");

            AssetDatabase.CreateAsset(so, path);
        }
    }
}
