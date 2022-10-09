// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#if BAKINGSHEET_BETTERSTREAMINGASSETS

using System.Collections.Generic;
using System.IO;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet.Unity
{
    public class StreamingAssetsFileSystem : IFileSystem
    {
        public StreamingAssetsFileSystem()
        {
            BetterStreamingAssets.Initialize();
        }

        public virtual bool Exists(string path)
        {
            return BetterStreamingAssets.FileExists(path);
        }

        public virtual IEnumerable<string> GetFiles(string path, string extension)
        {
            return BetterStreamingAssets.GetFiles(path, $"*.{extension}");
        }

        public virtual Stream OpenRead(string path)
        {
            return BetterStreamingAssets.OpenRead(path);
        }

        public virtual void CreateDirectory(string path)
        {
            // write access to streaming assets is not allowed
            throw new System.NotImplementedException();
        }

        public virtual Stream OpenWrite(string path)
        {
            // write access to streaming assets is not allowed
            throw new System.NotImplementedException();
        }
    }
}

#endif