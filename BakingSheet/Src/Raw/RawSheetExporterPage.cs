using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cathei.BakingSheet.Raw
{
    public interface IRawSheetExporterPage
    {
        void SetCell(int col, int row, string data);
    }

    public static class RawSheetExporterPageExtensions
    {
        private static bool ShouldExport(PropertyInfo prop)
        {
            if (prop.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;

            if (prop.SetMethod == null)
                return false;

            return true;
        }

        public static void Export(this IRawSheetExporterPage page, RawSheetConverter exporter, SheetConvertingContext context, ISheet sheet)
        {
            if (sheet.Count == 0)
                return;

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

            PropertyInfo[] sheetRowProperties = null, sheetElemProperties = null;

            int pageRow = 1;

            foreach (ISheetRow sheetRow in sheet.Values)
            {
                if (sheetRowProperties == null)
                {
                    sheetRowProperties = sheetRow.GetType()
                        .GetProperties(bindingFlags)
                        .Where(ShouldExport)
                        .OrderBy(x => x.Name == nameof(ISheetRow.Id))
                        .ToArray();

                    for (int i = 0; i < sheetRowProperties.Length; ++i)
                    {
                        var prop = sheetRowProperties[i];
                        page.SetCell(i, 0, prop.Name);
                    }
                }

                for (int i = 0; i < sheetRowProperties.Length; ++i)
                {
                    var prop = sheetRowProperties[i];
                    var value = prop.GetValue(sheetRow);
                    var cellValue = exporter.ValueToString(context, prop.PropertyType, value);

                    page.SetCell(i, pageRow, cellValue);
                }

                if (sheetRow is ISheetRowArray sheetRowArray)
                {
                    foreach (var sheetElem in sheetRowArray.Arr)
                    {
                        if (sheetElemProperties == null)
                        {
                            sheetElemProperties = sheetElem.GetType()
                                .GetProperties(bindingFlags)
                                .Where(ShouldExport)
                                .ToArray();

                            for (int i = 0; i < sheetElemProperties.Length; ++i)
                            {
                                var prop = sheetElemProperties[i];
                                page.SetCell(sheetRowProperties.Length + i, 0, prop.Name);
                            }
                        }

                        for (int i = 0; i < sheetElemProperties.Length; ++i)
                        {
                            var prop = sheetElemProperties[i];
                            var value = prop.GetValue(sheetElem);
                            var cellValue = exporter.ValueToString(context, prop.PropertyType, value);

                            page.SetCell(sheetRowProperties.Length + i, pageRow, cellValue);
                        }

                        pageRow += 1;
                    }
                }
                else
                {
                    pageRow += 1;
                }
            }
        }
    }
}
