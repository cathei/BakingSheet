using System.Threading.Tasks;

namespace Cathei.BakingSheet
{
    public interface ISheetExporter
    {
        Task<bool> Export(SheetConvertingContext context);
    }
}
