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
    public class PropertyMap
    {
        public abstract class Node
        {
            public delegate object GetterDelegate(object obj, object key);
            public delegate void SetterDelegate(object obj, object key, object value);
            public delegate IEnumerable<Attribute> AttributesGetterDelegate(Type attribute);
            public delegate object ModifyDelegate(object original);

            public Node Parent { get; set; }
            public string FullPath { get; set; }

            public Type IndexType { get; set; }
            public Type ValueType { get; set; }

            public GetterDelegate Getter { get; set; }
            public SetterDelegate Setter { get; set; }
            public AttributesGetterDelegate AttributesGetter { get; set; }

            public virtual bool IsLeaf => false;
            public virtual bool IsVertical => false;
            public abstract Node GetChild(string subpath);
            public virtual bool HasSubpath(string subpath) => false;

            // can be used when parent and child shares same column name
            public virtual Node ColumnNode => this;

            public abstract void UpdateIndex(object obj);
            public abstract IEnumerable<Node> TraverseChildren(List<object> indexes);
            public abstract void GenerateChildren(Func<Node, bool> isLeaf, int depth);

            protected virtual object GetChildIndex(int vindex, IEnumerator<object> indexer)
            {
                if (IndexType == null)
                    return null;

                indexer.MoveNext();
                return indexer.Current;
            }

            protected string AppendIndex(int depth)
            {
                return $"{FullPath}{Config.Delimiter}{{{depth}}}";
            }

            public virtual int GetVerticalCount(ISheetRow row, IEnumerator<object> indexer)
            {
                return Parent?.GetVerticalCount(row, indexer) ?? 1;
            }

            public object GetValue(ISheetRow row, int vindex, IEnumerator<object> indexer)
            {
                object obj = row;

                if (Parent != null)
                    obj = Parent.GetValue(row, vindex, indexer);

                if (obj == null)
                    return null;

                object index = Parent?.GetChildIndex(vindex, indexer);

                return Getter(obj, index);
            }

            public void SetValue(ISheetRow row, int vindex, IEnumerator<object> indexer, object value)
            {
                ModifyValue(row, vindex, indexer, _ => value);
            }

            public void ModifyValue(ISheetRow row, int vindex, IEnumerator<object> indexer, ModifyDelegate modifier)
            {
                object obj = null;

                if (Parent == null)
                {
                    obj = Getter(row, null);
                    obj = modifier(obj);

                    // there'd be no setter for root node
                    // Setter(row, null, obj);
                    return;
                }

                Parent.ModifyValue(row, vindex, indexer, parentObj =>
                {
                    if (parentObj == null)
                        return null;

                    object index = Parent?.GetChildIndex(vindex, indexer);

                    obj = Getter(parentObj, index);

                    // for leaf nodes there might be no default constructor available
                    if (obj == null && !IsLeaf)
                        obj = Activator.CreateInstance(ValueType);

                    obj = modifier(obj);

                    Setter(parentObj, index, obj);

                    return parentObj;
                });
            }
        }

        public class NodeObject : Node
        {
            private Dictionary<string, Node> _children;

            public override bool IsLeaf => _children == null;

            public override Node GetChild(string subpath) => _children?[subpath];

            public override bool HasSubpath(string subpath) => _children?.ContainsKey(subpath) ?? false;

            private string AppendPath(string subpath)
            {
                if (FullPath == null)
                    return subpath;

                return $"{FullPath}{Config.Delimiter}{subpath}";
            }

            public override void UpdateIndex(object obj)
            {
                if (IsLeaf)
                    return;

                foreach (var child in _children.Values)
                {
                    var elem = child.Getter(obj, null);
                    child.UpdateIndex(elem);
                }
            }

            public override IEnumerable<Node> TraverseChildren(List<object> indexes)
            {
                if (IsLeaf)
                {
                    yield return this;
                    yield break;
                }

                // Id column should come first
                if (_children.TryGetValue(nameof(ISheetRow.Id), out var idChild))
                {
                    foreach (var node in idChild.TraverseChildren(indexes))
                        yield return node;
                }

                foreach (var child in _children.Values)
                {
                    if (child == idChild)
                        continue;

                    foreach (var node in child.TraverseChildren(indexes))
                        yield return node;
                }
            }

            public override void GenerateChildren(Func<Node, bool> isLeaf, int depth)
            {
                if (isLeaf(this))
                    return;

                _children = new Dictionary<string, Node>();

                foreach (PropertyInfo propertyInfo in ValueType.GetProperties(BindingFlag))
                {
                    if (!ShouldInclude(propertyInfo))
                        continue;

                    var child = CreateNode(propertyInfo.PropertyType);

                    child.Parent = this;
                    child.ValueType = propertyInfo.PropertyType;
                    child.FullPath = AppendPath(propertyInfo.Name);
                    child.Getter = (obj, key) => propertyInfo.GetValue(obj);
                    child.Setter = (obj, key, value) => propertyInfo.SetValue(obj, value);
                    child.AttributesGetter = att => propertyInfo.GetCustomAttributes(att);

                    child.GenerateChildren(isLeaf, depth);

                    _children.Add(propertyInfo.Name, child);
                }
            }
        }

        public class NodeList : Node
        {
            private Node _child;
            private int _maxCount = 1;

            private readonly bool _isVertical;

            public override bool IsVertical => _isVertical;
            public override Node ColumnNode => _isVertical ? _child.ColumnNode : this;
            public override Node GetChild(string subpath) => _child;

            public NodeList(bool isVertical)
            {
                _isVertical = isVertical;
            }

            public override void UpdateIndex(object obj)
            {
                if (obj is IList list)
                {
                    _maxCount = Math.Max(_maxCount, list.Count);

                    foreach (var elem in list)
                        _child.UpdateIndex(elem);
                }
            }

            protected override object GetChildIndex(int vindex, IEnumerator<object> indexer)
            {
                if (_isVertical)
                {
                    // convert 0-base to 1-base
                    return vindex + 1;
                }

                return base.GetChildIndex(vindex, indexer);
            }

            public override int GetVerticalCount(ISheetRow row, IEnumerator<object> indexer)
            {
                if (_isVertical)
                {
                    // get vertical list count
                    var obj = GetValue(row, 0, indexer);

                    if (obj is IList list && list.Count > 0)
                        return list.Count;

                    return 1;
                }

                return base.GetVerticalCount(row, indexer);
            }

            public override IEnumerable<Node> TraverseChildren(List<object> indexes)
            {
                if (_isVertical)
                {
                    // no need to loop through indexes for vertical list
                    foreach (var node in _child.TraverseChildren(indexes))
                        yield return node;
                    yield break;
                }

                int current = indexes.Count;
                indexes.Add(null);

                // use 1-base for indexes
                for (int i = 1; i <= _maxCount; ++i)
                {
                    indexes[current] = i;

                    foreach (var node in _child.TraverseChildren(indexes))
                        yield return node;
                }

                indexes.RemoveAt(current);
            }

            public override void GenerateChildren(Func<Node, bool> isLeaf, int depth)
            {
                var elementType = GetGenericArgument(ValueType, typeof(IList<>))[0];

                IndexType = typeof(int);

                var child = CreateNode(elementType);

                child.Parent = this;
                child.ValueType = elementType;
                child.FullPath = _isVertical ? FullPath : AppendIndex(depth);

                child.Getter = (obj, key) =>
                {
                    if (obj is IList list)
                    {
                        // convert 1-base to 0-base
                        int idx = (int)key - 1;

                        if (idx < list.Count)
                            return list[idx];
                    }

                    return null;
                };

                child.Setter = (obj, key, value) =>
                {
                    if (obj is IList list)
                    {
                        // convert 1-base to 0-base
                        int idx = (int)key - 1;

                        while (list.Count <= idx)
                            list.Add(elementType.IsValueType ? Activator.CreateInstance(elementType) : null);

                        list[idx] = value;
                    }
                };

                child.AttributesGetter = AttributesGetter;

                child.GenerateChildren(isLeaf, _isVertical ? depth : depth + 1);

                _child = child;
            }
        }

        public class NodeDictionary : Node
        {
            private Node _child;

            private HashSet<object> PossibleKeys { get; set; }

            public override Node GetChild(string subpath) => _child;

            public override void UpdateIndex(object obj)
            {
                if (PossibleKeys == null)
                    PossibleKeys = new HashSet<object>();

                if (obj is IDictionary dict)
                {
                    foreach (var key in dict.Keys)
                        PossibleKeys.Add(key);

                    foreach (var elem in dict.Values)
                        _child.UpdateIndex(elem);
                }
            }

            public override IEnumerable<Node> TraverseChildren(List<object> indexes)
            {
                if (PossibleKeys == null)
                    yield break;

                int current = indexes.Count;
                indexes.Add(null);

                foreach (var key in PossibleKeys)
                {
                    indexes[current] = key;

                    foreach (var node in _child.TraverseChildren(indexes))
                        yield return node;
                }

                indexes.RemoveAt(current);
            }

            public override void GenerateChildren(Func<Node, bool> isLeaf, int depth)
            {
                var arguments = GetGenericArgument(ValueType, typeof(IDictionary<,>));
                var keyType = arguments[0];
                var elementType = arguments[1];

                IndexType = keyType;

                var child = CreateNode(elementType);

                child.Parent = this;
                child.ValueType = elementType;
                child.FullPath = AppendIndex(depth);

                child.Getter = (obj, key) =>
                {
                    if (obj is IDictionary dict)
                        return dict[key];
                    return null;
                };

                child.Setter = (obj, key, value) =>
                {
                    if (obj is IDictionary dict)
                        dict[key] = value;
                };

                child.AttributesGetter = AttributesGetter;

                child.GenerateChildren(isLeaf, depth + 1);

                _child = child;
            }
        }

        private static Node CreateNode(Type type)
        {
            if (typeof(IVerticalList).IsAssignableFrom(type))
            {
                return new NodeList(true);
            }
            if (typeof(IList).IsAssignableFrom(type))
            {
                return new NodeList(false);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return new NodeDictionary();
            }
            else
            {
                return new NodeObject();
            }
        }

        private static bool ShouldInclude(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return true;
        }

        private const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

        private NodeObject Root { get; set; }
        private NodeList Arr { get; set; }

        private readonly SheetConvertingContext _context;

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

        public PropertyMap(SheetConvertingContext context, Type sheetType, Func<Node, bool> isLeaf)
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

            Root.GenerateChildren(isLeaf, 0);

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

                Arr.GenerateChildren(isLeaf, 0);
            }
        }

        private List<object> _indexes = new List<object>();

        private HashSet<string> _warned = null;

        public void SetValue(ISheetRow row, int vindex, string path, string value, Func<Type, string, object> converter)
        {
            Node node = null;

            _indexes.Clear();

            bool isVertical = false;;

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
