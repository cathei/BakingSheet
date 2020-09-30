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
}
