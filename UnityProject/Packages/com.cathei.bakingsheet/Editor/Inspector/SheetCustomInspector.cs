// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    [CustomEditor(typeof(SheetScriptableObject))]
    public class SheetCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}