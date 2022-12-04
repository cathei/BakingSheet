// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Default implementation for contract resolver that determines and caches value converter.
    /// </summary>
    public class SheetContractResolver : ISheetContractResolver
    {
        public static readonly SheetContractResolver Instance = new SheetContractResolver();

        private readonly IReadOnlyList<ISheetValueConverter> _converters;

        private readonly ThreadLocal<ConverterCache> _cache = new ThreadLocal<ConverterCache>(() => new ConverterCache());

        private class ConverterCache
        {
            public readonly Dictionary<Type, ISheetValueConverter?> FromAttribute = new Dictionary<Type, ISheetValueConverter?>();
            public readonly Dictionary<Type, ISheetValueConverter?> FromTargetType = new Dictionary<Type, ISheetValueConverter?>();
        }

        /// <summary>
        /// Create default contract resolver with additional value converters.
        /// If you do not use any additional converter, use singleton Instance instead.
        /// </summary>
        /// <param name="converters">Additional value converters.</param>
        public SheetContractResolver(params ISheetValueConverter[] converters)
        {
            var list = new List<ISheetValueConverter>(converters)
            {
                // add default converters
                new NullableValueConverter(),
                new EnumValueConverter(),
                new PrimitiveValueConverter(),
                new DateTimeValueConverter(),
                new TimeSpanValueConverter(),
                new SheetReferenceValueConverter(),
                new AssetPathValueConverter()
            };

            _converters = list;
        }

        public virtual ISheetValueConverter? GetValueConverter(Type type)
        {
            var cache = _cache.Value!;

            if (cache.FromTargetType.TryGetValue(type, out var cached))
                return cached;

            // type-level converter
            if (type.IsDefined(typeof(SheetValueConverterAttribute)))
            {
                var attr = type.GetCustomAttribute<SheetValueConverterAttribute>()!;
                var converter = GetConverterFromAttribute(attr);

                cache.FromTargetType.Add(type, converter);
                return converter;
            }

            // sheet-level converter
            foreach (var converter in _converters)
            {
                if (!converter.CanConvert(type))
                    continue;

                cache.FromTargetType.Add(type, converter);
                return converter;
            }

            // there is no ValueConverter provided for this type
            cache.FromTargetType.Add(type, null);
            return null;
        }

        public virtual ISheetValueConverter? GetValueConverter(PropertyInfo? propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            if (!propertyInfo.IsDefined(typeof(SheetValueConverterAttribute)))
                return null;

            // property-level converter
            var attr = propertyInfo.GetCustomAttribute<SheetValueConverterAttribute>()!;
            var converter = GetConverterFromAttribute(attr);

            return converter;
        }

        private ISheetValueConverter? GetConverterFromAttribute(SheetValueConverterAttribute attr)
        {
            var cache = _cache.Value!;

            if (cache.FromAttribute.TryGetValue(attr.ConverterType, out var converter))
                return converter;

            converter = (ISheetValueConverter?)Activator.CreateInstance(attr.ConverterType);
            cache.FromAttribute.Add(attr.ConverterType, converter);
            return converter;
        }
    }
}
