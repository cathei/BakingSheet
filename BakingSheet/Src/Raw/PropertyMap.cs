using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

            public object GetInternal(ISheetRow row, IEnumerator<int> indexIter, bool create)
            {
                object obj = null;

                if (NodeType == NodeType.Object)
                {
                    if (Parent == null)
                        return row;

                    obj = Parent.GetInternal(row, indexIter, create);

                    if (obj == null)
                        return null;

                    var value = Property.GetValue(obj);

                    if (!create)
                        return value;
                    
                    if (value == null)
                    {
                        // create value
                        var elem = Activator.CreateInstance(Element);
                        Property.SetValue(obj, elem);

                        return elem;
                    }
                }
                else if (NodeType == NodeType.List)
                {
                    if (Parent == null)
                        obj = (row as ISheetRowArray).Arr;
                    else
                        obj = Parent.GetInternal(row, indexIter, create);

                    if (obj == null)
                        return null;

                    var list = obj as IList;

                    indexIter.MoveNext();
                    var idx = indexIter.Current;

                    if (idx < list.Count)
                        return list[idx];

                    if (create)
                    {
                        // create value
                        while (list.Count <= idx)
                        {
                            var elem = Activator.CreateInstance(Element);
                            list.Add(elem);
                        }

                        return list[idx];
                    }
                }

                return null;
            }

            public void Set(ISheetRow row, List<int> indexes, object value)
            {
                var obj = Parent.GetInternal(row, indexes.GetEnumerator(), true);
                Property.SetValue(obj, value);
            }

            public object Get(ISheetRow row, List<int> indexes)
            {
                return GetInternal(row, indexes.GetEnumerator(), false);
            }
        }

        private const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

        private Node Root { get; set; }
        private Node Arr { get; set; }

        private RawSheetImporter _importer;
        private SheetConvertingContext _context;

        internal static IEnumerable<string> ParseFlattenPath(string path)
        {
            const char delimeter = '.';

            int idx = 0;
            int next = path.IndexOf(delimeter);

            while (next != -1)
            {
                yield return path.Substring(idx, next);

                idx = next + 1;
                next = path.IndexOf(delimeter, idx);
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
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                    return type.GetGenericArguments();
                type = type.BaseType;
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

                string fullPath = parent.FullPath == null ? propertyInfo.Name : $"{parent.FullPath}.{propertyInfo.Name}";

                if (typeof(IList).IsAssignableFrom(propertyType))
                {
                    Type elementType = GetGenericArgument(propertyType, typeof(IList<>))[0];

                    node = new Node
                    {
                        FullPath = $"{fullPath}.{{{parent.ListDepth}}}",
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
                        _indexes.Add(arrIndex);
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

            if (row is ISheetRowArray rowArray)
                UpdateCountInternal(Arr, rowArray.Arr);
        }

        private void UpdateCountInternal(Node node, object obj)
        {
            if (node.NodeType == NodeType.List)
            {
                var list = obj as IList;

                node.MaxCount = Math.Max(node.MaxCount, list.Count);

                foreach (var elem in list)
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

                foreach (var child in node.Children.Values)
                {
                    var prop = child.Property.GetValue(obj);

                    if (prop != null)
                        UpdateCountInternal(child, prop);
                }
            }
        }

        // UpdateCount is required to get correct result
        // index list are returned just to feed back, only valid on enumeration loop
        public IEnumerable<(Node, int, List<int>)> TraverseLeaf()
        {
            _indexes.Clear();

            foreach (var node in TraverseInternal(Root))
                yield return (node, 0, _indexes);

            if (Arr != null)
            {
                foreach (var node in TraverseInternal(Arr))
                    yield return (node, _indexes[0], _indexes);
            }
        }

        private IEnumerable<Node> TraverseInternal(Node node)
        {
            bool isLeaf = _importer.IsConvertable(node.Element);

            if (node.NodeType == NodeType.List)
            {
                int current = _indexes.Count;

                _indexes.Add(0);

                for (int i = 0; i < node.MaxCount; ++i)
                {
                    _indexes[current] = i;

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
