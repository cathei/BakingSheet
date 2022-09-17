// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public partial class ScriptableObjectSheetConverter : ISheetImporter
    {
        private string _loadPath;
        private IFileSystem _fileSystem;

        public ScriptableObjectSheetConverter(string path, IFileSystem fileSystem = null)
        {
            _loadPath = path;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        public Task<bool> Import(SheetConvertingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
