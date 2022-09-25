// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Cathei.BakingSheet.Editor
{
    [CustomPropertyDrawer(typeof(UnitySheetReferenceAttribute))]
    public class SheetReferenceInnerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get TValue of Sheet<TKey, TValue>.Reference
            var targetTypeInfo = fieldInfo.DeclaringType.GenericTypeArguments[1].FullName;

            // position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            var content = new GUIContent
            {
                text = "(None)"
            };

            var selectedObject = property.objectReferenceValue;

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
                var dropdown = new SheetReferenceDropdown(property, label.text, targetTypeInfo, new AdvancedDropdownState());
                dropdown.Show(position);
            }
        }
    }
}