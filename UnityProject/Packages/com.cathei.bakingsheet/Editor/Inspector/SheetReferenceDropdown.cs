// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Cathei.BakingSheet.Editor
{
    public class SheetReferenceDropdown : AdvancedDropdown
    {
        private readonly SerializedProperty _property;
        private readonly string _label;
        private readonly string _targetTypeInfo;

        // private List<UnityEngine.Object> _selections = new List<UnityEngine.Object>();

        public SheetReferenceDropdown(
            SerializedProperty property, string label, string targetTypeInfo, AdvancedDropdownState state) : base(state)
        {
            _property = property;
            _label = label;
            _targetTypeInfo = targetTypeInfo;

            var minSize = base.minimumSize;
            minSize.y = 300f;
            base.minimumSize = minSize;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_label);

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

            _property.objectReferenceInstanceIDValue = item.id;
            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}