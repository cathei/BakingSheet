using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetImporter : ISheetImporter
    {
        protected abstract Task<bool> LoadData();
        protected abstract RawSheetImporterPage GetPage(string sheetName);

        public TimeZoneInfo TimeZoneInfo { get; }

        public RawSheetImporter(TimeZoneInfo timeZoneInfo)
        {
            TimeZoneInfo = timeZoneInfo;
        }

        public async Task<bool> Import(SheetConvertingContext context)
        {
            var success = await LoadData();

            if (!success)
            {
                context.Logger.LogError($"Failed to load data");
                return false;
            }

            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                var page = GetPage(prop.Name);
                if (page == null)
                {
                    context.Logger.LogError($"Failed to find sheet: {prop.Name}");
                    continue;
                }

                var rawSheet = new RawSheet(page);
                var sheet = Activator.CreateInstance(prop.PropertyType) as Sheet;

                rawSheet.WriteToSheet(this, context, sheet);
                prop.SetValue(context.Container, sheet);

                sheet.Name = prop.Name;
                context.Container.AllSheets.Add(sheet);
            }
 
            return true;
        }

        public virtual object StringToValue(SheetConvertingContext context, Type type, string value)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            
            if (typeof(ISheetReference).IsAssignableFrom(type))
            {
                var reference = Activator.CreateInstance(type) as ISheetReference;
                reference.Id = StringToValue(context, reference.IdType, value);
                return reference;
            }

            if(typeof(DateTime).IsAssignableFrom(type))
            {
                var local = DateTime.Parse(value);
                return TimeZoneInfo.ConvertTimeToUtc(local, TimeZoneInfo);
            }

            if(typeof(TimeSpan).IsAssignableFrom(type))
            {
                return TimeSpan.Parse(value);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                var underlyingType = Nullable.GetUnderlyingType(type);
                return StringToValue(context, underlyingType, value);
            }

            return Convert.ChangeType(value, type);
        }
    }
}
