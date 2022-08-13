using System.Collections.Generic;

namespace AssetStoreTools.Uploader
{
    public static class PackageViewStorer
    {
        private static readonly Dictionary<string, PackageView> SavedPackages = new Dictionary<string, PackageView>();
        
        internal static PackageView GetPackage(PackageData packageData)
        {
            string versionId = packageData.VersionId;
            if (SavedPackages.ContainsKey(versionId))
            {
                var savedPackage = SavedPackages[versionId];
                savedPackage.UpdateDataValues(packageData);
                return savedPackage;
            }

            var package = new PackageView(packageData);
            SavedPackages.Add(package.VersionId, package);
            return package;
        }

        public static void Reset()
        {
            SavedPackages.Clear();
        }
    }
}