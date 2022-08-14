using System.Collections.Generic;
using System.IO;
using Cathei.BakingSheet.Internal;
using Better.StreamingAssets;

namespace Cathei.BakingSheet
{
    public class StreamingAssetsFileSystem : IFileSystem
    {
        public StreamingAssetsFileSystem()
            => BetterStreamingAssets.Initialize();

        public bool Exists(string path)
            => BetterStreamingAssets.FileExists(path);

        public IEnumerable<string> GetFiles(string path, string extension)
            => BetterStreamingAssets.GetFiles(path, $"*.{extension}");

        public Stream OpenRead(string path)
            => BetterStreamingAssets.OpenRead(path);

        // write access to streaming assets is not allowed
        public void CreateDirectory(string path)
            => throw new System.NotImplementedException();

        // write access to streaming assets is not allowed
        public Stream OpenWrite(string path)
            => throw new System.NotImplementedException();
    }
}