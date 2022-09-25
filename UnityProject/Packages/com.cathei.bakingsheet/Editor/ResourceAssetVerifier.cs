// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Reflection;

namespace Cathei.BakingSheet.Unity
{
    public class ResourcePathVerifier : SheetVerifier<ResourcePath>
    {
        // any string column with ResourceAttribute will be passed through the verify process
        // return value is the error string
        public override string Verify(PropertyInfo propertyInfo, ResourcePath assetPath)
        {
            if (!assetPath.IsValid())
                return null;

            // var fullPath = assetPath.FullPath;

            var obj = assetPath.Load<UnityEngine.Object>();
            if (obj != null)
                return null;

            return $"Resource {assetPath.FullPath} not found!";
        }
    }
}