using System;
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
        private readonly string _targetTypeInfo;

        public SheetReferenceDropdown(string targetTypeInfo, AdvancedDropdownState state) : base(state)
        {
            _targetTypeInfo = targetTypeInfo;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Sheet Reference");

            var nullItem = new AdvancedDropdownItem("None");
            nullItem.id = -1;
            root.AddChild(nullItem);

            var assetGuids = AssetDatabase.FindAssets($"t:{nameof(SheetScriptableObject)}");

            foreach (var assetGuid in assetGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var sheetSO = AssetDatabase.LoadAssetAtPath<SheetScriptableObject>(assetPath);

                if (sheetSO.typeInfo != _targetTypeInfo)
                    continue;

                foreach (var rowSO in sheetSO.Rows)
                {
                    var dropdownItem = new AdvancedDropdownItem(rowSO.name);
                    root.AddChild(dropdownItem);
                }
            }

            return root;
        }
    }
}