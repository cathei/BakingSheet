// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Collections.Generic;
using System.IO;

namespace Cathei.BakingSheet.Internal
{
    public interface IFileSystem
    {
        IEnumerable<string> GetFiles(string path, string extension);

        bool Exists(string path);
        Stream OpenRead(string path);

        void CreateDirectory(string path);
        Stream OpenWrite(string path);
    }

    public class FileSystem : IFileSystem
    {
        public virtual IEnumerable<string> GetFiles(string path, string extension)
        {
            return Directory.GetFiles(path, $"*.{extension}");
        }

        public virtual bool Exists(string path)
        {
            return File.Exists(path);
        }

        public virtual Stream OpenRead(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public virtual void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public virtual Stream OpenWrite(string path)
        {
            return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }
    }
}
