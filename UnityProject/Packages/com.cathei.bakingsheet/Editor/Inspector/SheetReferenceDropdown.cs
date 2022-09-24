using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cathei.BakingSheet.Editor
{
    public class SheetReferenceDropdown : AdvancedDropdown
    {
        private readonly SerializedProperty _property;
        private readonly SerializedProperty _assetProp;
        private readonly string _targetTypeInfo;

        // private List<UnityEngine.Object> _selections = new List<UnityEngine.Object>();

        public SheetReferenceDropdown(
            SerializedProperty property, AdvancedDropdownState state) : base(state)
        {
            _property = property;
            _assetProp = property.FindPropertyRelative("asset");

            var typeInfoProp = property.FindPropertyRelative("typeInfo");
            _targetTypeInfo = typeInfoProp.stringValue;

            var minSize = base.minimumSize;
            minSize.y = 300f;
            base.minimumSize = minSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_assetProp.displayName);

            var nullItem = new AdvancedDropdownItem("(None)");
            nullItem.id = 0;
            root.AddChild(nullItem);

            var assetGuids = AssetDatabase.FindAssets($"t:{nameof(SheetScriptableObject)}");

            foreach (var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var sheetSO = AssetDatabase.LoadAssetAtPath<SheetScriptableObject>(assetPath);

                if (sheetSO.typeInfo != _targetTypeInfo)
                    continue;

                root.AddSeparator();

                foreach (var rowSO in sheetSO.Rows)
                {
                    var dropdownItem = new AdvancedDropdownItem(rowSO.name)
                    {
                        id = rowSO.GetInstanceID(),
                        icon = AssetPreview.GetMiniThumbnail(rowSO)
                    };

                    root.AddChild(dropdownItem);
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            _assetProp.objectReferenceInstanceIDValue = item.id;
            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}