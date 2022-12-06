// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Internal
{
    /// <summary>
    /// Tree structure represents assignable properties in sheet rows.
    /// </summary>
    public partial class PropertyMap
    {
        private PropertyNodeObject Root { get; }
        private PropertyNodeList Arr { get; }

        private readonly SheetConvertingContext _context;

        private readonly List<object> _indexes = new List<object>();

        private HashSet<string> _warned = null;

        private int _maxDepth;

        public int MaxDepth => _maxDepth;

        private static IEnumerable<string> ParseFlattenPath(string path)
        {
            int idx = 0;
            int next = path.IndexOf(Config.IndexDelimiter, StringComparison.Ordinal);

            while (next != -1)
            {
                yield return path.Substring(idx, next - idx);

                idx = next + 1;
                next = path.IndexOf(Config.IndexDelimiter, idx, StringComparison.Ordinal);
            }

            yield return path.Substring(idx);
        }

        internal static Type[] GetGenericArgument(Type type, Type baseType)
        {
            if (baseType.IsInterface)
            {
                // just use the first interface available
                foreach (var impl in type.GetInterfaces())
                {
                    if (impl.IsGenericType && impl.GetGenericTypeDefinition() == baseType)
                        return impl.GetGenericArguments();
                }
            }
            else
            {
                Type current = type;

                while (current != null)
                {
                    if (current.IsGenericType && current.GetGenericTypeDefinition() == baseType)
                        return current.GetGenericArguments();
                    current = current.BaseType;
                }
            }

            throw new InvalidOperationException($"Type {type} does not implement {baseType}");
        }

        private static bool RootGetter(PropertyNode child, object obj, object key, out object value)
        {
            value = obj;
            return true;
        }

        public PropertyMap(SheetConvertingContext context, Type sheetType)
        {
            _context = context;

            var resolver = context.Container.ContractResolver;
            var rowType = GetGenericArgument(sheetType, typeof(Sheet<,>))[1];

            Root = new PropertyNodeObject(null, null, rowType, RootGetter, null, null, resolver, 0);

            _maxDepth = Root.CalculateDepth();

            if (typeof(ISheetRowArray).IsAssignableFrom(rowType))
            {
                var arrPropertyInfo = Config.GetRowArrayProperty(rowType);

                Debug.Assert(arrPropertyInfo != null);

                Arr = new PropertyNodeList(null, null, arrPropertyInfo.PropertyType,
                    PropertyNodeObject.ValueGetter, null, arrPropertyInfo, resolver, 0, true);

                _maxDepth = Math.Max(_maxDepth, Arr.CalculateDepth());
            }
        }

        /// <summary>
        /// Set value of a specific property of a row.
        /// </summary>
        /// <param name="row">Target row.</param>
        /// <param name="vindex">Vertical index of the row.</param>
        /// <param name="path">Path to the node (Column name).</param>
        /// <param name="value">Value to assign.</param>
        /// <param name="formatter">Format provider to convert value to object.</param>
        public void SetValue(ISheetRow row, int vindex, string path, string value, ISheetFormatter formatter)
        {
            PropertyNode node = null;

            var resolver = _context.Container.ContractResolver;
            var context = new SheetValueConvertingContext(formatter, resolver);

            _indexes.Clear();

            bool isVertical = false;

            foreach (var subpath in ParseFlattenPath(path))
            {
                if (node == null)
                {
                    if (Root.HasSubpath(subpath))
                    {
                        node = Root.ColumnNode;
                    }
                    else if (Arr != null && Arr.ColumnNode.HasSubpath(subpath))
                    {
                        node = Arr.ColumnNode;
                        isVertical = true;
                    }
                    else
                    {
                        _warned = _warned ?? new HashSet<string>();

                        if (!_warned.Contains(path))
                        {
                            _context.Logger.LogError("Column name is invalid");
                            _warned.Add(path);
                        }
                        return;
                    }
                }

                if (node.IndexType != null)
                {
                    object index = context.StringToValue(node.IndexType, subpath);
                    _indexes.Add(index);
                }

                node = node.GetChild(subpath);

                Debug.Assert(node != null);

                if (node.IsVertical)
                {
                    if (isVertical)
                    {
                        _context.Logger.LogError("Nested vertical list is not supported");
                        return;
                    }

                    isVertical = true;
                }

                node = node.ColumnNode;
            }

            if (!isVertical && vindex != 0)
            {
                _context.Logger.LogError("There is multiple value for a non-vertical column");
                return;
            }

            Debug.Assert(node != null);

            var converter = node.ValueConverter;

            if (converter == null)
            {
                _context.Logger.LogError("No converter registered for type {NodeType}", node.ValueType);
                return;
            }

            node.SetValue(row, vindex, _indexes.GetEnumerator(),
                converter.StringToValue(node.ValueType, value, context));
        }

        /// <summary>
        /// Updating possible index and keys to match the rows in target sheet
        /// </summary>
        /// <param name="sheet">Target sheet</param>
        public void UpdateIndex(ISheet sheet)
        {
            foreach (var row in sheet)
            {
                Root.UpdateIndex(row);

                if (row is ISheetRowArray rowArray)
                    Arr.UpdateIndex(rowArray.Arr);
            }
        }

        /// <summary>
        /// Traverse each leaf node
        /// Calling UpdateCount first is required to get correct result
        /// index list are returned just to feed back, only valid on enumeration loop
        /// </summary>
        public IEnumerable<(PropertyNode, IReadOnlyList<object>)> TraverseLeaf()
        {
            _indexes.Clear();

            foreach (var node in Root.TraverseChildren(_indexes))
                yield return (node, _indexes);

            if (Arr != null)
            {
                foreach (var node in Arr.TraverseChildren(_indexes))
                    yield return (node, _indexes);
            }
        }
    }
}
