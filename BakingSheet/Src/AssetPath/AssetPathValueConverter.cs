// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;

namespace Cathei.BakingSheet.Internal
{
    public class AssetPathValueConverter : SheetValueConverter<ISheetAssetPath>
    {
        protected override ISheetAssetPath StringToValue(Type type, string value, SheetValueConvertingContext context)
        {
            ISheetAssetPath assetPath = (ISheetAssetPath)Activator.CreateInstance(type);

            if (string.IsNullOrEmpty(value))
            {
                assetPath.FullPath = null;
                return assetPath;
            }

            assetPath.FullPath = $"{assetPath.Prefix}{value}{assetPath.Postfix}";
            return assetPath;
        }

        protected override string ValueToString(Type type, ISheetAssetPath value, SheetValueConvertingContext context)
        {
            return ExtractPath(value);
        }

        public static string ExtractPath(ISheetAssetPath value)
        {
            if (string.IsNullOrEmpty(value?.FullPath))
                return null;

            string path = value.FullPath;

            int start = 0, end = path.Length;

            if (path.StartsWith(value.Prefix))
                start += value.Prefix.Length;

            if (path.EndsWith(value.Postfix))
                end -= value.Postfix.Length;

            return path.Substring(start, end - start);
        }
    }
}
