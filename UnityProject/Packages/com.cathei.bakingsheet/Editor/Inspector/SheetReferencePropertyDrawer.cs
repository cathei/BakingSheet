// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEditor;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    [CustomPropertyDrawer(typeof(Sheet<,>.Reference), true)]
    public class SheetReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetProp = property.FindPropertyRelative("asset");

            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            EditorGUI.PropertyField(position, assetProp, new GUIContent(property.displayName));
        }
    }
}