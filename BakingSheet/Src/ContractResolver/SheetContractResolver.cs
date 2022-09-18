// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public class SheetContractResolver : ISheetContractResolver
    {
        public static readonly SheetContractResolver Instance = new SheetContractResolver();

        private readonly IReadOnlyList<ISheetValueConverter> _converters;

        private readonly ThreadLocal<ConverterCache> _cache = new ThreadLocal<ConverterCache>(() => new ConverterCache());

        private class ConverterCache
        {
            public readonly Dictionary<Type, ISheetValueConverter> FromAttribute = new Dictionary<Type, ISheetValueConverter>();
            public readonly Dictionary<Type, ISheetValueConverter> FromTargetType = new Dictionary<Type, ISheetValueConverter>();
        }

        public SheetContractResolver() : this(null) { }

        protected SheetContractResolver(IEnumerable<ISheetValueConverter> converters)
        {
            var list = new List<ISheetValueConverter>();

            if (converters != null)
                list.AddRange(converters);

            // add default converters
            list.Add(new EnumValueConverter());
            list.Add(new PrimitiveValueConverter());
            list.Add(new DateTimeValueConverter());
            list.Add(new TimeSpanValueConverter());
            list.Add(new SheetReferenceValueConverter());

            _converters = list;
        }

        public virtual ISheetValueConverter GetValueConverter(Type type)
        {
            var cache = _cache.Value;

            if (cache.FromTargetType.TryGetValue(type, out var cached))
                return cached;

            // nullable support
            Type innerType = Nullable.GetUnderlyingType(type);
            if (innerType == null)
                innerType = type;

            // type-level converter
            if (innerType.IsDefined(typeof(SheetValueConverterAttribute)))
            {
                var attr = innerType.GetCustomAttribute<SheetValueConverterAttribute>();
                var converter = GetConverterFromAttribute(attr);

                cache.FromTargetType.Add(type, converter);
                return converter;
            }

            // sheet-level converter
            foreach (var converter in _converters)
            {
                if (!converter.CanConvert(innerType))
                    continue;

                cache.FromTargetType.Add(type, converter);
                return converter;
            }

            // there is no ValueConverter provided for this type
            cache.FromTargetType.Add(type, null);
            return null;
        }

        public virtual ISheetValueConverter GetValueConverter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return null;

            if (!propertyInfo.IsDefined(typeof(SheetValueConverterAttribute)))
                return null;

            // property-level converter
            var attr = propertyInfo.GetCustomAttribute<SheetValueConverterAttribute>();
            var converter = GetConverterFromAttribute(attr);

            return converter;
        }

        private ISheetValueConverter GetConverterFromAttribute(SheetValueConverterAttribute attr)
        {
            var cache = _cache.Value;

            if (cache.FromAttribute.TryGetValue(attr.ConverterType, out var converter))
                return converter;

            converter = (ISheetValueConverter)Activator.CreateInstance(attr.ConverterType);
            cache.FromAttribute.Add(attr.ConverterType, converter);
            return converter;
        }
    }
}
