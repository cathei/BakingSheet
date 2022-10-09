// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if BAKINGSHEET_ADDRESSABLES

using System.Collections.Generic;
using System.Reflection;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// Verifies if asset at resource path exists.
    /// </summary>
    public class AddressablePathVerifier : SheetVerifier<AddressablePath>
    {
        private readonly Dictionary<string, AddressableAssetEntry> _addressToEntry;

        public AddressablePathVerifier()
        {
            // "Addressable" doesn't have to way to find entry by address, seriously?
            _addressToEntry = new Dictionary<string, AddressableAssetEntry>();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var buffer = new List<AddressableAssetEntry>();

            foreach (var group in settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    buffer.Clear();

                    entry.GatherAllAssets(buffer, true, true, true);

                    // still have to loop and gather sub assets
                    // because if addressable has folder entry it will not add sub asset of the asset
                    int count = buffer.Count;

                    for (int i = 0; i < count; ++i)
                        buffer[i].GatherAllAssets(buffer, false, false, true);

                    foreach (var subEntry in buffer)
                        _addressToEntry[subEntry.address] = subEntry;
                }
            }
        }

        public override string Verify(PropertyInfo propertyInfo, AddressablePath assetPath)
        {
            if (!assetPath.IsValid())
                return null;

            if (_addressToEntry.ContainsKey(assetPath.FullPath))
                return null;

            return $"Addressable {assetPath.FullPath} not found!";
        }
    }
}

#endif