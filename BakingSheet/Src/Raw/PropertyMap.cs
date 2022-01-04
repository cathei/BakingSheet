using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Raw
{
    // property map for a sheet
    public class PropertyMap
    {
        public enum NodeType
        {
            Object,
            List
        }

        // node represent a property
        public class Node
        {
            public NodeType NodeType { get; set; }
            public string FullPath { get; set; }

            public PropertyInfo Property { get; set; }
            public Type Element { get; set; }

            public Node Parent { get; set; }
            public Dictionary<string, Node> Children { get; set; }

            // used to organize columns when export
            public int MaxCount { get; set; } = 1;

            // used for path formatting
            public int ListDepth { get; set; } = 0;

            public void Add(string path, Node child)
            {
                if (Children == null)
                    Children = new Dictionary<string, Node>();

                child.Parent = this;
                Children.Add(path, child);
            }

            public object GetInternal(ISheetRow row, ref List<int>.Enumerator indexer)
            {
                object obj;

                if (NodeType == NodeType.Object)
                {
                    if (Parent == null)
                        return row;

                    obj = Parent.GetInternal(row, ref indexer);

                    if (obj == null)
                        return null;

                    var value = Property.GetValue(obj);

                    return value;
                }
                else if (NodeType == NodeType.List)
                {
                    if (Parent == null)
                        obj = row;
                    else
                        obj = Parent.GetInternal(row, ref indexer);

                    if (obj == null)
                        return null;

                    var list = Property.GetValue(obj) as IList;

                    if (list == null)
                        return null;

                    indexer.MoveNext();

                    // convert 1-base to 0-base
                    var idx = indexer.Current - 1;

                    if (idx < list.Count)
                        return list[idx];
                }

                return null;
            }

            private delegate void ModifierDelegate(object obj, ref List<int>.Enumerator idxer);

            private void PropagateInternal(ISheetRow row, ref List<int>.Enumerator indexer, ModifierDelegate modifier)
            {
                if (NodeType == NodeType.Object)
                {
                    if (Parent == null)
                    {
                        modifier(row, ref indexer);
                        return;
                    }

                    Parent.PropagateInternal(row, ref indexer, (object obj, ref List<int>.Enumerator idxer) =>
                    {
                        if (obj == null)
                            return;

                        var value = Property.GetValue(obj) ?? Activator.CreateInstance(Element);

                        modifier(value, ref idxer);

                        // incase of value-type struct, set back to original variable
                        if (Element.IsValueType)
                            Property.SetValue(obj, value);
                    });
                }
                else if (NodeType == NodeType.List)
                {
                    ModifierDelegate listModifier = (object obj, ref List<int>.Enumerator idxer) =>
                    {
                        if (obj == null)
                            return;

                        var list = Property.GetValue(obj) as IList;

                        if (list == null)
                        {
                            list = Activator.CreateInstance(Property.PropertyType) as IList;
                            Property.SetValue(obj, list);
                        }

                        idxer.MoveNext();

                        // convert 1-base to 0-base
                        var idx = idxer.Current - 1;

                        // create value
                        while (list.Count <= idx)
                            list.Add(Activator.CreateInstance(Element));

                        var value = list[idx] ?? Activator.CreateInstance(Element);
                        modifier(value, ref idxer);

                        if (Element.IsValueType)
                            list[idx] = value;
                    };

                    if (Parent == null)
                        listModifier(row, ref indexer);
                    else
                        Parent.PropagateInternal(row, ref indexer, listModifier);
                }
            }

            public void Set(ISheetRow row, List<int> indexes, object value)
            {
                var indexer = indexes.GetEnumerator();

                Parent.PropagateInternal(row, ref indexer, (object obj, ref List<int>.Enumerator idxer) =>
                {
                    if (NodeType == NodeType.Object)
                    {
                        Property.SetValue(obj, value);
                    }
                    else
                    {
                        var list = Property.GetValue(obj) as IList;

                        if (list == null)
                        {
                            list = Activator.CreateInstance(Property.PropertyType) as IList;
                            Property.SetValue(obj, list);
                        }

                        idxer.MoveNext();

                        // convert 1-base to 0-base
                        var idx = idxer.Current - 1;

                        // set null for default since some of class like string does not have default constructor
                        while (list.Count <= idx)
                            list.Add(Element.IsValueType ? Activator.CreateInstance(Element) : null);

                        list[idx] = value;
                    }
                });
            }

            public object Get(ISheetRow row, List<int> indexes)
            {
                var indexer = indexes.GetEnumerator();
                return GetInternal(row, ref indexer);
            }
        }

        private const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

        private Node Root { get; set; }
        private Node Arr { get; set; }

        private RawSheetImporter _importer;
        private SheetConvertingContext _context;

        internal static IEnumerable<string> ParseFlattenPath(string path)
        {
            int idx = 0;
            int next = path.IndexOf(Config.Delimiter);

            while (next != -1)
            {
                yield return path.Substring(idx, next - idx);

                idx = next + 1;
                next = path.IndexOf(Config.Delimiter, idx);
            }

            yield return path.Substring(idx);
        }

        private bool ShouldInclude(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return true;
        }

        private Type[] GetGenericArgument(Type type, Type baseType)
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

        public PropertyMap(RawSheetImporter importer, SheetConvertingContext context, ISheet sheet)
        {
            _importer = importer;
            _context = context;

            Type rowType = GetGenericArgument(sheet.GetType(), typeof(Sheet<,>))[1];

            Root = new Node
            {
                FullPath = null,
                Property = null,
                Element = rowType,
                NodeType = NodeType.Object,
                ListDepth = 0,
            };

            GenerateChildren(Root);

            if (typeof(ISheetRowArray).IsAssignableFrom(rowType))
            {
                Type arrElementType = GetGenericArgument(rowType, typeof(SheetRowArray<,>))[1];
                PropertyInfo arrPropertyInfo = rowType.GetProperty(nameof(ISheetRowArray.Arr));

                Arr = new Node
                {
                    FullPath = null,
                    Property = arrPropertyInfo,
                    Element = arrElementType,
                    NodeType = NodeType.List,
                    ListDepth = 1,
                };

                GenerateChildren(Arr);
            }
        }

        private void GenerateChildren(Node parent)
        {
            // TODO: Prevent cycle!

            foreach (PropertyInfo propertyInfo in parent.Element.GetProperties(BindingFlag))
            {
                if (!ShouldInclude(propertyInfo))
                    continue;

                Type propertyType = propertyInfo.PropertyType;
                Node node = null;

                string fullPath = parent.FullPath == null ? propertyInfo.Name : $"{parent.FullPath}{Config.Delimiter}{propertyInfo.Name}";

                if (typeof(IList).IsAssignableFrom(propertyType))
                {
                    if (propertyType.IsArray)
                    {
                        _context.Logger.LogError("Array is not supported! Use List instead.");
                        continue;
                    }

                    Type elementType = GetGenericArgument(propertyType, typeof(IList<>))[0];

                    node = new Node
                    {
                        FullPath = $"{fullPath}{Config.Delimiter}{{{parent.ListDepth}}}",
                        Property = propertyInfo,
                        Element = elementType,
                        NodeType = NodeType.List,
                        ListDepth = parent.ListDepth + 1
                    };
                }
                else
                {
                    node = new Node
                    {
                        FullPath = fullPath,
                        Property = propertyInfo,
                        Element = propertyType,
                        NodeType = NodeType.Object,
                        ListDepth = parent.ListDepth
                    };
                }

                parent.Add(propertyInfo.Name, node);

                if (!_importer.IsConvertable(node.Element))
                    GenerateChildren(node);
            }
        }

        private List<int> _indexes = new List<int>();

        public void SetValue(ISheetRow row, int arrIndex, string path, string value)
        {
            Node node = null;

            _indexes.Clear();

            foreach (var subpath in ParseFlattenPath(path))
            {
                if (node == null)
                {
                    if (Root.Children.ContainsKey(subpath))
                    {
                        if (arrIndex != 0)
                        {
                            _context.Logger.LogError("There is multiple value for a single column");
                            return;
                        }

                        node = Root;
                    }
                    else
                    {
                        if (Arr == null || !Arr.Children.ContainsKey(subpath))
                        {
                            _context.Logger.LogError("Column name is invalid", path);
                            return;
                        }

                        node = Arr;

                        // convert 0-base to 1-base
                        _indexes.Add(arrIndex + 1);
                    }
                }

                if (int.TryParse(subpath, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    _indexes.Add(index);
                    continue;
                }

                node = node.Children[subpath];
            }

            if (!_importer.IsConvertable(node.Element))
            {
                _context.Logger.LogError("Type {PropertyType} is not convertable", node.Element);
                return;
            }

            try
            {
                node.Set(row, _indexes, _importer.StringToValue(node.Element, value));
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to convert value \"{CellValue}\" of type {PropertyType}", value, node.Element);
            }
        }

        public void UpdateCount(ISheetRow row)
        {
            UpdateCountInternal(Root, row);

            if (Arr != null)
                UpdateCountInternal(Arr, row);
        }

        private void UpdateCountInternal(Node node, object obj)
        {
            if (node.NodeType == NodeType.List)
            {
                IList list = node.Property.GetValue(obj) as IList;

                if (list == null)
                    return;

                // root array count is meant to vary per row
                // so we will keep max count as 1 here
                if (node.Parent != null)
                    node.MaxCount = Math.Max(node.MaxCount, list.Count);

                if (node.Children == null)
                    return;

                foreach (object elem in list)
                {
                    if (elem == null)
                        continue;

                    foreach (var child in node.Children.Values)
                    {
                        UpdateCountInternal(child, elem);
                    }
                }
            }
            else
            {
                if (node.Children == null)
                    return;

                if (node.Property != null)
                    obj = node.Property.GetValue(obj);

                if (obj == null)
                    return;

                foreach (Node child in node.Children.Values)
                {
                    UpdateCountInternal(child, obj);
                }
            }
        }

        // UpdateCount is required to get correct result
        // index list are returned just to feed back, only valid on enumeration loop
        public IEnumerable<(Node, bool, List<int>)> TraverseLeaf()
        {
            _indexes.Clear();

            foreach (var node in TraverseInternal(Root))
                yield return (node, false, _indexes);

            if (Arr != null)
            {
                foreach (var node in TraverseInternal(Arr))
                    yield return (node, true, _indexes);
            }
        }

        private IEnumerable<Node> TraverseInternal(Node node)
        {
            bool isLeaf = _importer.IsConvertable(node.Element);

            if (node.NodeType == NodeType.List)
            {
                int current = _indexes.Count;

                // convert 0-base to 1-base
                _indexes.Add(1);

                for (int i = 0; i < node.MaxCount; ++i)
                {
                    // convert 0-base to 1-base
                    _indexes[current] = i + 1;

                    if (isLeaf)
                    {
                        yield return node;
                    }
                    else
                    {
                        foreach (Node child in node.Children.Values)
                        {
                            foreach (Node result in TraverseInternal(child))
                                yield return result;
                        }
                    }
                }

                _indexes.RemoveAt(current);
            }
            else
            {
                if (isLeaf)
                {
                    yield return node;
                }
                else
                {
                    foreach (Node child in node.Children.OrderByDescending(x => x.Key == "Id").Select(x => x.Value))
                    {
                        foreach (Node result in TraverseInternal(child))
                            yield return result;
                    }
                }
            }
        }
    }
}
