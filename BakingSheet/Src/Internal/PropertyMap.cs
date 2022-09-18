// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
        private NodeObject Root { get; set; }
        private NodeList Arr { get; set; }

        private readonly SheetConvertingContext _context;

        internal static IEnumerable<string> ParseFlattenPath(string path)
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

        public PropertyMap(SheetConvertingContext context, Type sheetType, ISheetContractResolver resolver)
        {
            _context = context;

            Type rowType = GetGenericArgument(sheetType, typeof(Sheet<,>))[1];

            Root = new NodeObject
            {
                FullPath = null,
                ValueType = rowType,
                Getter = (obj, key) => obj,
                Setter = null,
                AttributesGetter = _ => Enumerable.Empty<Attribute>(),
            };

            Root.GenerateChildren(resolver, 0);

            if (typeof(ISheetRowArray).IsAssignableFrom(rowType))
            {
                Type arrElementType = GetGenericArgument(rowType, typeof(SheetRowArray<,>))[1];
                PropertyInfo arrPropertyInfo = rowType.GetProperty(nameof(ISheetRowArray.Arr));

                Arr = new NodeList(true)
                {
                    FullPath = null,
                    ValueType = arrPropertyInfo.PropertyType,
                    Getter = (obj, key) => arrPropertyInfo.GetValue(obj),
                    Setter = null,
                    AttributesGetter = _ => Enumerable.Empty<Attribute>(),
                };

                Arr.GenerateChildren(resolver, 0);
            }
        }

        private List<object> _indexes = new List<object>();

        private HashSet<string> _warned = null;

        public void SetValue(ISheetRow row, int vindex, string path, string value, ISheetFormatter formatter)
        {
            Node node = null;

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
                    object index = converter(node.IndexType, subpath);
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

            node.SetValue(row, vindex, _indexes.GetEnumerator(), converter(node.ValueType, value));
        }

        public void UpdateIndex(ISheet sheet)
        {
            foreach (var row in sheet)
            {
                Root.UpdateIndex(row);

                if (row is ISheetRowArray rowArray)
                    Arr.UpdateIndex(rowArray.Arr);
            }
        }

        // UpdateCount is required to get correct result
        // index list are returned just to feed back, only valid on enumeration loop
        public IEnumerable<(Node, IEnumerable<object>)> TraverseLeaf()
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
