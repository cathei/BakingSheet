// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Unity;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public interface IUnitySheetReference : ISheetReference
    {
        public SheetRowScriptableObject Asset { get; set; }
    }

    internal class UnitySheetReferenceAttribute : PropertyAttribute { }

    public partial class Sheet<TKey, TValue>
    {
        [Serializable]
        public partial struct Reference : IUnitySheetReference
        {
            [SerializeField, UnitySheetReference] private SheetRowScriptableObject asset;

            SheetRowScriptableObject IUnitySheetReference.Asset
            {
                get => asset;
                set => asset = value;
            }

            partial void EnsureLoadReference()
            {
                if (asset == null)
                    return;

                reference = asset.GetRow<TValue>();
                Id = reference.Id;
            }
        }
    }
}
