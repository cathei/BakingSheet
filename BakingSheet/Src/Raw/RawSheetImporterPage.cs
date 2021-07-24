using System.Linq;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetImporterPage
    {
        public abstract string GetCell(int col, int row);

        public void GetSize(out int numColumns, out int numRows)
        {
            int col = 0, row = 0;

            // First row is configuration variables
            // First column is data keys
            // Find number of configuration variables
            while (true)
            {
                var value = GetCell(col, 0);
                if (string.IsNullOrEmpty(value))
                    break;

                ++col;
            }

            numColumns = col;

            // Find number of data keys
            while (true)
            {
                bool isEmptyRow = Enumerable.Range(0, numColumns).All(i => {
                    var value = GetCell(i, row + 1);
                    return string.IsNullOrEmpty(value);
                });

                if (isEmptyRow)
                    break;

                ++row;
            }

            numRows = row;
        }
    }
}
