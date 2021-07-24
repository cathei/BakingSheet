using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public interface ISheetImporter
    {
        Task<bool> Import(SheetConvertingContext context);
    }
}
