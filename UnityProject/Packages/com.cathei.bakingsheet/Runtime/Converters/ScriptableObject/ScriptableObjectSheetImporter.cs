// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public class ScriptableObjectSheetImporter : ISheetImporter
    {
        private readonly SheetContainerScriptableObject _so;

        public ScriptableObjectSheetImporter(SheetContainerScriptableObject so)
        {
            _so = so;
        }

        public Task<bool> Import(SheetConvertingContext context)
        {
            // _so.Sheets
            return Task.FromResult(true);
        }
    }
}
