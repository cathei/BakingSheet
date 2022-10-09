// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Unity
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
            var sheetProperties = context.Container.GetSheetProperties();

            foreach (var sheetSO in _so.Sheets)
            {
                using (context.Logger.BeginScope(sheetSO.name))
                {
                    string sheetName = sheetSO.name;

                    if (!sheetProperties.TryGetValue(sheetName, out var prop))
                    {
                        context.Logger.LogError("Failed to find sheet: {SheetName}", sheetName);
                        continue;
                    }

                    var sheet = (ISheet)Activator.CreateInstance(prop.PropertyType);

                    foreach (var rowSO in sheetSO.Rows)
                    {
                        using (context.Logger.BeginScope(rowSO.name))
                        {
                            var row = rowSO.GetRow(sheet.RowType);
                            sheet.Add(row);
                        }
                    }

                    prop.SetValue(context.Container, sheet);
                }
            }

            // _so.Sheets
            return Task.FromResult(true);
        }
    }
}
