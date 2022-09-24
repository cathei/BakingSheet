using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Cathei.BakingSheet.Editor
{
    [CustomPropertyDrawer(typeof(Sheet<,>.Reference), true)]
    public class SheetReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetProp = property.FindPropertyRelative("asset");

            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            var content = new GUIContent
            {
                text = "(None)"
            };

            var selectedObject = assetProp.objectReferenceValue;

            if (selectedObject != null)
            {
                content.text = selectedObject.name;
                content.image = AssetPreview.GetMiniThumbnail(selectedObject);
            }

            var labelButtonRect = position;
            labelButtonRect.xMax = position.xMax - 20f;

            var objectFieldStyle = new GUIStyle(EditorStyles.objectField);

            if (GUI.Button(labelButtonRect, content, objectFieldStyle) && selectedObject != null)
            {
                EditorGUIUtility.PingObject(selectedObject);
            }

            var listButtonRect = position;
            listButtonRect.xMin = labelButtonRect.xMax;
            listButtonRect = new RectOffset(-1, -1, -1, -1).Add(listButtonRect);

            var objectFieldButtonStyle = new GUIStyle("ObjectFieldButton");

            if (GUI.Button(listButtonRect, new GUIContent(""), objectFieldButtonStyle))
            {
                var dropdown = new SheetReferenceDropdown(property, new AdvancedDropdownState());
                dropdown.Show(position);
            }
        }
    }
}