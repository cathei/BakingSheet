using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetImporter : ISheetImporter
    {
        protected abstract Task<bool> LoadData();
        protected abstract IRawSheetImporterPage GetPage(string sheetName);

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
                context.Logger.LogError("Failed to load data");
                return false;
            }

            var sheetProps = context.Container.GetSheetProperties();

            foreach (var prop in sheetProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var page = GetPage(prop.Name);

                    if (page == null)
                        continue;

                    var sheet = Activator.CreateInstance(prop.PropertyType) as ISheet;

                    page.Import(this, context, sheet);
                    prop.SetValue(context.Container, sheet);
                }
            }

            return true;
        }

        public virtual object StringToValue(Type type, string value)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value, true);
            }

            if (typeof(ISheetReference).IsAssignableFrom(type))
            {
                var reference = Activator.CreateInstance(type) as ISheetReference;
                reference.Id = StringToValue(reference.IdType, value);
                return reference;
            }

            if (type == typeof(DateTime))
            {
                var local = DateTime.Parse(value);
                return TimeZoneInfo.ConvertTimeToUtc(local, TimeZoneInfo);
            }

            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(value);
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                    return null;
                return StringToValue(underlyingType, value);
            }

            return Convert.ChangeType(value, type);
        }
    }
}
