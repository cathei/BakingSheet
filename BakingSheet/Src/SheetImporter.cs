using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cathei.BakingSheet
{
    public interface ISheetImporter
    {
        Task<bool> Load();
        ISheetImporterData GetData(string sheetName);
    }

    public interface ISheetImporterData
    {
        string GetCell(int col, int row);
    }

    public static class SheetImporterDataExtensions
    {
        public static void GetSize(this ISheetImporterData data, out int numColumns, out int numRows)
        {
            int col = 0, row = 0;

            // First row is configuration variables
            // First column is data keys
            // Find number of configuration variables
            while (true)
            {
                var value = data.GetCell(col, 0);
                if (string.IsNullOrEmpty(value))
                    break;

                ++col;
            }

            numColumns = col;

            // Find number of data keys
            while (true)
            {
                bool isEmptyRow = Enumerable.Range(0, numColumns).All(i => {
                    var value = data.GetCell(i, row + 1);
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
