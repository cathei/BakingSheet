// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Xunit;
using Cathei.BakingSheet.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Cathei.BakingSheet.Tests
{
    public class TestFileSystem : IFileSystem, IDisposable
    {
        public IDictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();

        public void SetTestData(string path, string content)
        {
            files[path] = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public void VerifyTestData(string path, string expected)
        {
            Assert.Contains(path, files);

            var content = Encoding.UTF8.GetString(files[path].ToArray());
            Assert.Equal(expected, content, ignoreLineEndingDifferences: true);
        }

        public IEnumerable<string> GetFiles(string path, string extension)
        {
            return files.Keys.Where(x => x.StartsWith(path));
        }

        public bool Exists(string path)
        {
            return files.ContainsKey(path);
        }

        public Stream OpenRead(string path)
        {
            return files[path];
        }

        public void CreateDirectory(string path)
        {
            // do nothing
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
