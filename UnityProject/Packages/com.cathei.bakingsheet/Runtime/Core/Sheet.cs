// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Represents a single page of Sheet.
    /// </summary>
    /// <typeparam name="TKey">Type of Id column.</typeparam>
    /// <typeparam name="TValue">Type of Row.</typeparam>
    public abstract partial class Sheet<TKey, TValue> : KeyedCollection<TKey, TValue>, ISheet<TKey, TValue>
        where TValue : SheetRow<TKey>, new()
    {
        [Preserve]
        public string Name { get; set; }

        private PropertyMap _propertyMap;

        public Type RowType => typeof(TValue);

        public new TValue this[TKey id]
        {
            get
            {
                if (id == null || !Contains(id))
                    return default(TValue);
                return base[id];
            }
        }

        public TValue Find(TKey id) => this[id];

        bool ISheet.Contains(object key) => Contains((TKey)key);
        void ISheet.Add(object value) => Add((TValue)value);
        IEnumerator<ISheetRow> ISheet.GetEnumerator() => GetEnumerator();

        protected override TKey GetKeyForItem(TValue item)
        {
            return item.Id;
        }

        private PropertyMap GetPropertyMap(SheetConvertingContext context)
        {
            if (_propertyMap != null)
                return _propertyMap;

            _propertyMap = new PropertyMap(context, GetType());
            return _propertyMap;
        }

        PropertyMap ISheet.GetPropertyMap(SheetConvertingContext context) => GetPropertyMap(context);

        public virtual void PostLoad(SheetConvertingContext context)
        {
            var rowTypeToSheet = context.Container.GetSheetProperties().Values
                .Select(p => p.GetValue(context.Container) as ISheet)
                .Where(x => x != null)
                .ToDictionary(x => x.RowType);

            using (context.Logger.BeginScope(Name))
            {
                var propertyMap = GetPropertyMap(context);

                propertyMap.UpdateIndex(this);

                foreach (var (node, indexes) in propertyMap.TraverseLeaf())
                {
                    if (!typeof(ISheetReference).IsAssignableFrom(node.ValueType))
                        continue;

                    var referenceRowType = node.ValueType.GenericTypeArguments[1];

                    if (!rowTypeToSheet.TryGetValue(referenceRowType, out var sheet))
                    {
                        context.Logger.LogError("Failed to find sheet for {ReferenceType}", node.ValueType);
                        continue;
                    }

                    foreach (var row in Items)
                    {
                        int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                        using (context.Logger.BeginScope(row.Id))
                        using (context.Logger.BeginScope(node.FullPath, indexes))
                        {
                            for (int vindex = 0; vindex < verticalCount; ++vindex)
                            {
                                // only proceed when path is valid
                                if (!node.TryGetValue(row, vindex, indexes.GetEnumerator(), out var obj))
                                    continue;

                                if (obj is ISheetReference refer)
                                {
                                    refer.Map(context, sheet);
                                    node.SetValue(row, vindex, indexes.GetEnumerator(), obj);
                                }
                            }
                        }
                    }
                }

                foreach (var row in Items)
                {
                    using (context.Logger.BeginScope(row.Id))
                    {
                        row.PostLoad(context);
                    }
                }
            }
        }

        public virtual void VerifyAssets(SheetConvertingContext context)
        {
            using (context.Logger.BeginScope(Name))
            {
                var propertyMap = GetPropertyMap(context);

                propertyMap.UpdateIndex(this);

                foreach (var (node, indexes) in propertyMap.TraverseLeaf())
                {
                    foreach (var verifier in context.Verifiers)
                    {
                        if (!verifier.CanVerify(node.PropertyInfo, node.ValueType))
                            continue;

                        foreach (var row in Items)
                        {
                            using (context.Logger.BeginScope(row.Id))
                            using (context.Logger.BeginScope(node.FullPath, indexes))
                            {
                                int verticalCount = node.GetVerticalCount(row, indexes.GetEnumerator());

                                for (int vindex = 0; vindex < verticalCount; ++vindex)
                                {
                                    var obj = node.GetValue(row, vindex, indexes.GetEnumerator());
                                    var err = verifier.Verify(node.PropertyInfo, obj);

                                    if (err != null)
                                        context.Logger.LogError("Verification: {Error}", err);
                                }
                            }
                        }
                    }
                }

                foreach (var row in Items)
                {
                    using (context.Logger.BeginScope(row.Id))
                    {
                        row.VerifyAssets(context);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a single page of Sheet, with string Id.
    /// For other type of Id, use generic version.
    /// </summary>
    /// <typeparam name="T">Type of Row.</typeparam>
    public abstract class Sheet<T> : Sheet<string, T>
        where T : SheetRow<string>, new() {}
}
