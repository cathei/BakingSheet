// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Interface for any asset paths.
    /// They will serialized as string of raw value.
    /// All implementing class must have constructor with single string parameter.
    /// </summary>
    public interface ISheetAssetPath
    {
        /// <summary>
        /// Part of path that placed in cell of Datasheet.
        /// </summary>
        string RawValue { get; }

        /// <summary>
        /// Full path that combined with base path and extensions.
        /// </summary>
        string FullPath { get; }
    }
}
