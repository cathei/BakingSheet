// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Globalization;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetImporter : ISheetImporter
    {
        protected abstract Task<bool> LoadData();
        protected abstract IRawSheetImporterPage GetPage(string sheetName);

        public TimeZoneInfo TimeZoneInfo { get; }
        public IFormatProvider FormatProvider { get; }

        private bool _isLoaded;

        public RawSheetImporter(TimeZoneInfo timeZoneInfo, IFormatProvider formatProvider)
        {
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Utc;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }

        public virtual void Reset()
        {
            _isLoaded = false;
        }

        public async Task<bool> Import(SheetConvertingContext context)
        {
            if (!_isLoaded)
            {
                var success = await LoadData();

                if (!success)
                {
                    context.Logger.LogError("Failed to load data");
                    return false;
                }

                _isLoaded = true;
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

        internal bool IsConvertableNode(PropertyMap.Node node)
        {
            return IsConvertable(node.ValueType);
        }

        public virtual bool IsConvertable(Type type)
        {
            // is it numeric type?
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal))
                return true;

            // is it string or date type?
            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(TimeSpan))
                return true;

            // is it sheet reference?
            if (typeof(ISheetReference).IsAssignableFrom(type))
                return true;

            // is it nullable value type?
            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            return false;
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
                var local = DateTime.Parse(value, FormatProvider);
                return TimeZoneInfo.ConvertTimeToUtc(local, TimeZoneInfo);
            }

            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(value, FormatProvider);
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                    return null;
                return StringToValue(underlyingType, value);
            }

            return Convert.ChangeType(value, type, FormatProvider);
        }

        public virtual string ValueToString(Type type, object value)
        {

        }
    }
}
