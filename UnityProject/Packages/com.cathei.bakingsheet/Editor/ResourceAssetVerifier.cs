using System.IO;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class ResourceAssetVerifier : SheetVerifier<ResourceAttribute, string>
    {
        // any string column with ResourceAttribute will be passed through the verify process
        // return value is the error string
        public override string Verify(ResourceAttribute attribute, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = Path.Combine(attribute.Prefix, path);

            var obj = Resources.Load(path);
            if (obj != null)
                return null;

            return $"Resource {path} not found!";
        }
    }
}