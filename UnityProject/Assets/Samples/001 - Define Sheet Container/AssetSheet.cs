using System.Collections;
using System.Collections.Generic;
using Cathei.BakingSheet.Unity;
using UnityEngine;

namespace Cathei.BakingSheet.Examples
{
    public class AssetSheet : Sheet<AssetSheet.Row>
    {
        public class Row : SheetRow
        {
            public DirectAssetPath Direct { get; set; }
            public ResourcePath Resource { get; set; }
            public AddressablePath Addressable { get; set; }
        }
    }
}
