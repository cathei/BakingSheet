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
        public SheetReferenceDropdown(AdvancedDropdownState state) : base(state)
        {
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Sheet Reference");

            var nullItem = new AdvancedDropdownItem("None");
            nullItem.id = -1;
            root.AddChild(nullItem);

            return root;
        }
    }
}