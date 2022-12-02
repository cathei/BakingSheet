// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Internal
{
    /// <summary>
    /// Tree structure represents assignable properties in sheet rows.
    /// </summary>
    public partial class PropertyMap
    {
        private NodeObject Root { get; }
        private NodeList Arr { get; }

        private readonly SheetConvertingContext _context;

        private List<object> _indexes = new List<object>();

        private HashSet<string> _warned = null;

        private int _maxDepth;

        public int MaxDepth => _maxDepth;

        private static IEnumerable<string> ParseFlattenPath(string path)
        {
            int idx = 0;
            int next = path.IndexOf(Config.Delimiter, StringComparison.Ordinal);

            while (next != -1)
            {
                yield return path.Substring(idx, next - idx);

                idx = next + 1;
                next = path.IndexOf(Config.Delimiter, idx, StringComparison.Ordinal);
            }

            yield return path.Substring(idx);
        }

        private static bool ShouldInclude(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsDefined(typeof(NonSerializedAttribute)))
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return true;
        }

        private static Type[] GetGenericArgument(Type type, Type baseType)
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
                while (type != null)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                        return type.GetGenericArguments();
                    type = type.BaseType;
                }
            }

            return null;
        }

        private static bool RootGetter(Node child, object obj, object key, out object value)
        {
            value = obj;
            return true;
        }

        public PropertyMap(SheetConvertingContext context, Type sheetType)
        {
            _context = context;

            var resolver = context.Container.ContractResolver;

            Type rowType = GetGenericArgument(sheetType, typeof(Sheet<,>))[1];

            Root = new NodeObject
            {
                FullPath = null,
                ValueType = rowType,
                Getter = RootGetter,
                Setter = null,
                PropertyInfo = null,
            };

            Root.GenerateChildren(resolver, 0);

            _maxDepth = Root.CalculateDepth();

            if (typeof(ISheetRowArray).IsAssignableFrom(rowType))
            {
                Type arrElementType = GetGenericArgument(rowType, typeof(SheetRowArray<,>))[1];
                PropertyInfo arrPropertyInfo = rowType.GetProperty(nameof(ISheetRowArray.Arr));

                Arr = new NodeList(true)
                {
                    FullPath = null,
                    ValueType = arrPropertyInfo.PropertyType,
                    Getter = NodeObject.ValueGetter,
                    Setter = null,
                    PropertyInfo = arrPropertyInfo,
                };

                Arr.GenerateChildren(resolver, 0);

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
            Node node = null;

            var context = new SheetValueConvertingContext(formatter, _context.Container.ContractResolver);

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

            node.SetValue(row, vindex, _indexes.GetEnumerator(),
                node.ValueConverter.StringToValue(node.ValueType, value, context));
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
        public IEnumerable<(Node, IReadOnlyList<object>)> TraverseLeaf()
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
