using System;
using System.Threading.Tasks;

namespace Cathei.BakingSheet.Raw
{
    public abstract class RawSheetConverter : RawSheetImporter, ISheetExporter
    {
        protected RawSheetConverter(TimeZoneInfo timeZoneInfo) : base(timeZoneInfo)
        {

        }

        public abstract Task<bool> Export(SheetConvertingContext context);

        public virtual string ValueToString(SheetConvertingContext context, Type type, object value)
        {
            if (value == null)
                return null;

            if (value is ISheetReference)
            {
                var reference = value as ISheetReference;
                return ValueToString(context, reference.IdType, reference.Id);
            }

            if (value is DateTime)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc((DateTime)value, TimeZoneInfo);
                return local.ToString();
            }

            return value.ToString();
        }
    }
}
