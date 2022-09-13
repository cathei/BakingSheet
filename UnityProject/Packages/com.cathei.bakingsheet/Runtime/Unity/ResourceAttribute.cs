// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

namespace Cathei.BakingSheet
{
    public class ResourceAttribute : SheetAssetAttribute
    {
        public string Prefix { get; }

        public ResourceAttribute(string prefix = null)
        {
            Prefix = prefix;
        }
    }
}