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
                assetPath.RelativePath = null;
                return assetPath;
            }

            assetPath.RelativePath = value;
            return assetPath;
        }

        protected override string ValueToString(Type type, ISheetAssetPath value, SheetValueConvertingContext context)
        {
            if (!value.IsValid())
                return null;

            return value.RelativePath;
        }
    }
}
