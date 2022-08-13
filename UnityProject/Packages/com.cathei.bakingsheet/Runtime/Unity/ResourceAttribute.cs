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