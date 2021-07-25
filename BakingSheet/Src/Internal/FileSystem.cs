using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Internal
{
    public interface IFileSystem
    {
        IEnumerable<string> GetFiles(string path, string extension);

        Stream OpenRead(string path);
        Stream OpenWrite(string path);
    }

    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> GetFiles(string path, string extension)
        {
            return Directory.GetFiles(path, $"*.{extension}");
        }

        public Stream OpenRead(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public Stream OpenWrite(string path)
        {
            return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }
}
