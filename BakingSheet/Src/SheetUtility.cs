using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public static class SheetUtility
    {
        public static void MapReferences(SheetConvertingContext context, object obj)
        {
            var refProps = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => typeof(ISheetReference).IsAssignableFrom(p.PropertyType));

            foreach (var prop in refProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    var refer = prop.GetValue(obj) as ISheetReference;
                    refer.Map(context);
                    prop.SetValue(obj, refer);
                }
            }
        }

        public static void VerifyAssets(SheetConvertingContext context, object obj)
        {
            var assetProps = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute(typeof(SheetAssetAttribute)) != null);

            foreach (var prop in assetProps)
            {
                using (context.Logger.BeginScope(prop.Name))
                {
                    foreach (var verifier in context.Verifiers)
                    {
                        if (!verifier.TargetType.IsAssignableFrom(prop.PropertyType))
                            continue;

                        foreach (var att in prop.GetCustomAttributes(verifier.TargetAttribute))
                        {
                            var err = verifier.Verify(att, prop.GetValue(obj));
                            if (err != null)
                                context.Logger.LogError("Verification: {Error}", err);
                        }
                    }
                }
            }
        }
    }
}
