using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
            position = EditorGUI.PrefixLabel(position, label);

            var content = new GUIContent();
            content.text = "None";

            if (assetProp.objectReferenceValue == null)
                content.text = assetProp.objectReferenceValue.name;

            if (EditorGUI.DropdownButton(position, content, FocusType.Keyboard))
            {
                var dropdown = new SheetReferenceDropdown(new AdvancedDropdownState());
                dropdown.Show(position);
            }
        }
    }
}