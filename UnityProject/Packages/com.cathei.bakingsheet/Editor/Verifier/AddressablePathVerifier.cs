// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Reflection;

namespace Cathei.BakingSheet.Unity
{
    /// <summary>
    /// Verifies if asset at resource path exists.
    /// </summary>
    public class ResourcePathVerifier : SheetVerifier<ResourcePath>
    {
        public override string Verify(PropertyInfo propertyInfo, ResourcePath assetPath)
        {
            if (!assetPath.IsValid())
                return null;

            var obj = assetPath.Load<UnityEngine.Object>();
            if (obj != null)
                return null;

            return $"Resource {assetPath.FullPath} not found!";
        }
    }
}