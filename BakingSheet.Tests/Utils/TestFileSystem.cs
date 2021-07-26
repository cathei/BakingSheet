using System;
using Xunit;
using Cathei.BakingSheet.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cathei.BakingSheet.Tests
{
    public class TestFileSystem : IFileSystem, IDisposable
    {
        public Dictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();

        public void SetTestData(string path, string content)
        {
            files[path] = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public void VerifyTestData(string path, string expected)
        {
            Assert.True(files.ContainsKey(path));

            var content = Encoding.UTF8.GetString(files[path].ToArray());
            Assert.Equal(expected, content);
        }

        public IEnumerable<string> GetFiles(string path, string extension)
        {
            return files.Keys;
        }

        public bool Exists(string path)
        {
            return files.ContainsKey(path);
        }

        public Stream OpenRead(string path)
        {
            return files[path];
        }

        public Stream OpenWrite(string path)
        {
            files[path] = new MemoryStream();
            return files[path];
        }

        public void Dispose()
        {
            foreach (var stream in files.Values)
                stream.Dispose();
        }
    }
}
