using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public class RawSheetRow : List<Dictionary<string, string>>
    {
        public override string ToString()
        {
            var infos = this.SelectMany(x => x)
                .GroupBy(x => x.Key)
                .Select(g => $"{g.Key}: {string.Join(",", g.Select(x => x.Value))}");

            return string.Join(",", infos);
        }

        internal void WriteToSheetRow(RawSheetImporter importer, SheetConvertingContext context, ISheetRow obj)
        {
            var parentTag = context.Tag;

            context.SetTag(parentTag, obj.Id);

            WriteToObject(importer, context, obj, 0);

            if (obj is ISheetRowArray rowArr)
            {
                var elemType = rowArr.ElemType;

                for (int i = 0; i < Count; ++i)
                {
                    context.SetTag(parentTag, obj.Id, i);

                    var elem = Activator.CreateInstance(elemType);
                    WriteToObject(importer, context, elem, i);
                    rowArr.Arr.Add(elem);
                }
            }
        }

        private void WriteToObject(RawSheetImporter importer, SheetConvertingContext context, object obj, int index)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

            var parentTag = context.Tag;

            foreach (var item in this[0])
            {
                var prop = type.GetProperty(item.Key, bindingFlags);
                if (prop == null)
                    continue;

                context.SetTag(parentTag, item.Key);

                try
                {
                    object value = importer.StringToValue(context, prop.PropertyType, item.Value);
                    prop.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"[{context.Tag}] Failed to convert value \"{item.Value}\" of type {prop.PropertyType}\n{ex}");
                }
            }
        }
    }
}
