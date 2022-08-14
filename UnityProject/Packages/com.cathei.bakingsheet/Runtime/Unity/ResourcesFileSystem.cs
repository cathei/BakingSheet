using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Cathei.BakingSheet.Internal;
using System.IO;
using System.Linq;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Rough file system implementation to support Resources folder.
    /// It is recommended to use StreamingAssets for most cases.
    /// </summary>
    public class ResourcesFileSystem : IFileSystem
    {
        private readonly Dictionary<string, TextAsset> _textAssets;

        public ResourcesFileSystem()
        {
            _textAssets = new Dictionary<string, TextAsset>();
        }

        public IEnumerable<string> GetFiles(string path, string extension)
        {
            // there is no way to get true path from resources folder
            // for example this will loss the nested path to the asset
            // also no way to check extension
            var assets = Resources.LoadAll<TextAsset>(path);

            foreach (var asset in assets)
                _textAssets[Path.Combine(path, asset.name)] = asset;

            return _textAssets.Keys.Where(x => x.StartsWith(path));
        }

        private TextAsset LoadAndCache(string path)
        {
            if (_textAssets.TryGetValue(path, out var asset))
                return asset;

            asset = Resources.Load<TextAsset>(path);
            if (asset == null)
                return null;

            _textAssets[path] = asset;
            return asset;
        }

        public bool Exists(string path)
        {
            return LoadAndCache(path) != null;
        }

        public Stream OpenRead(string path)
        {
            var asset = LoadAndCache(path);

            if (asset == null)
                throw new FileNotFoundException(path);

            return new MemoryStream(asset.bytes);
        }

        public void CreateDirectory(string path)
        {
            // write access to resources is not supported
            throw new NotImplementedException();
        }

        public Stream OpenWrite(string path)
        {
            // write access to resources is not supported
            throw new NotImplementedException();
        }
    }
}
