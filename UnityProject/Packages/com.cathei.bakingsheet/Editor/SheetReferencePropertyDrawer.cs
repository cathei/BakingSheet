using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cathei.BakingSheet.Editor
{
    [CustomPropertyDrawer(typeof(Sheet<,>.Reference), true)]
    public class SheetReferencePropertyDrawer : PropertyDrawer
    {
        // public override VisualElement CreatePropertyGUI(SerializedProperty property)
        // {
        //     return new PropertyField(property.FindPropertyRelative("asset"));
        // }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetProp = property.FindPropertyRelative("asset");

            EditorGUI.PropertyField(position, assetProp, label);
        }
    }
}