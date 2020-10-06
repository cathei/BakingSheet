namespace Cathei.BakingSheet
{
    public class PrefabAttribute : SheetAssetAttribute
    {
        public string Path { get; }

        public PrefabAttribute(string path)
        {
            Path = path;
        }
    }
}