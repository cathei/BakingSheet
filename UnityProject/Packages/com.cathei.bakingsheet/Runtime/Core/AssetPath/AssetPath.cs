// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public interface ISheetAssetPath
    {
        string FullPath { get; set; }

        string Prefix { get; }
        string Postfix { get; }
    }

    public class AssetPath : ISheetAssetPath
    {
        [Preserve]
        public string FullPath { get; set; }

        public virtual string Prefix => string.Empty;
        public virtual string Postfix => string.Empty;
    }
}
