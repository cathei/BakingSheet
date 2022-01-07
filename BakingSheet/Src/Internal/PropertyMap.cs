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
            public abstract Node GetChild(string subpath);
            public virtual bool HasSubpath(string subpath) => false;

            public abstract void UpdateIndex(object obj);
            public abstract IEnumerable<Node> TraverseChildren(List<object> indexes);
            public abstract void GenerateChildren(Func<Type, bool> isLeaf, int depth);

            protected string AppendPath(string subpath)
            {
                if (FullPath == null)
                    return subpath;

                return $"{FullPath}{Config.Delimiter}{subpath}";
            }

            protected string AppendIndex(int depth)
            {
                // for root array ignore depth
                if (Parent == null)
                    return null;

                if (FullPath == null)
                    return "{{{depth}}}";

                return $"{FullPath}{Config.Delimiter}{{{depth}}}";
            }

            public object GetValue(ISheetRow row, IEnumerator<object> indexer)
            {
                object obj = row;

                if (Parent != null)
                    obj = Parent.GetValue(row, indexer);

                object index = null;

                if (Parent?.IndexType != null)
                {
                    indexer.MoveNext();
                    index = indexer.Current;
                }

                if (obj == null)
                    return null;

                return Getter(obj, index);
            }

            public void SetValue(ISheetRow row, IEnumerator<object> indexer, object value)
            {
                ModifyValue(row, indexer, _ => value);
            }

            public void ModifyValue(ISheetRow row, IEnumerator<object> indexer, ModifyDelegate modifier)
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

                Parent.ModifyValue(row, indexer, parentObj =>
                {
                    object index = null;

                    if (Parent?.IndexType != null)
                    {
                        indexer.MoveNext();
                        index = indexer.Current;
                    }

                    if (parentObj == null)
                        return null;

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
            private Dictionary<string, Node> Children { get; set; }

            public override bool IsLeaf => Children == null;

            public override Node GetChild(string subpath) => Children?[subpath];

            public override bool HasSubpath(string subpath) => Children?.ContainsKey(subpath) ?? false;

            public override void UpdateIndex(object obj)
            {
                if (IsLeaf)
                    return;

                foreach (var child in Children.Values)
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

                foreach (var child in Children.OrderByDescending(x => x.Key == "Id").Select(x => x.Value))
                {
                    foreach (var node in child.TraverseChildren(indexes))
                        yield return node;
                }
            }

            public override void GenerateChildren(Func<Type, bool> isLeaf, int depth)
            {
                if (isLeaf(ValueType))
                    return;

                Children = new Dictionary<string, Node>();

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

                    Children.Add(propertyInfo.Name, child);
                }
            }
        }

        public class NodeList : Node
        {
            public Node Child { get; set; }

            private int MaxCount { get; set; } = 1;

            public override Node GetChild(string subpath) => Child;

            public override void UpdateIndex(object obj)
            {
                if (obj is IList list)
                {
                    // for root array size varies, so keep 1 as max count
                    if (Parent != null)
                        MaxCount = Math.Max(MaxCount, list.Count);

                    foreach (var elem in list)
                        Child.UpdateIndex(elem);
                }
            }

            public override IEnumerable<Node> TraverseChildren(List<object> indexes)
            {
                int current = indexes.Count;
                indexes.Add(null);

                for (int i = 0; i < MaxCount; ++i)
                {
                    // convert 0-base to 1-base
                    indexes[current] = i + 1;

                    foreach (var node in Child.TraverseChildren(indexes))
                        yield return node;
                }

                indexes.RemoveAt(current);
            }

            public override void GenerateChildren(Func<Type, bool> isLeaf, int depth)
            {
                var elementType = GetGenericArgument(ValueType, typeof(IList<>))[0];

                IndexType = typeof(int);

                var child = CreateNode(elementType);

                child.Parent = this;
                child.ValueType = elementType;
                child.FullPath = AppendIndex(depth);

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

                child.GenerateChildren(isLeaf, depth + 1);

                Child = child;
            }
        }

        public class NodeDictionary : Node
        {
            private Node Child { get; set; }

            private HashSet<object> PossibleKeys { get; set; }

            public override Node GetChild(string subpath) => Child;

            public override void UpdateIndex(object obj)
            {
                if (PossibleKeys == null)
                    PossibleKeys = new HashSet<object>();

                if (obj is IDictionary dict)
                {
                    foreach (var key in dict.Keys)
                        PossibleKeys.Add(key);

                    foreach (var elem in dict.Values)
                        Child.UpdateIndex(elem);
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

                    foreach (var node in Child.TraverseChildren(indexes))
                        yield return node;
                }

                indexes.RemoveAt(current);
            }

            public override void GenerateChildren(Func<Type, bool> isLeaf, int depth)
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

                Child = child;
            }
        }

        private static Node CreateNode(Type type)
        {
            if (typeof(IList).IsAssignableFrom(type))
            {
                return new NodeList();
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

        public PropertyMap(SheetConvertingContext context, Type sheetType, Func<Type, bool> isLeaf)
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

                Arr = new NodeList
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

        public void SetValue(ISheetRow row, int arrIndex, string path, string value, Func<Type, string, object> converter)
        {
            Node node = null;

            _indexes.Clear();

            foreach (var subpath in ParseFlattenPath(path))
            {
                if (node == null)
                {
                    if (Root.HasSubpath(subpath))
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
                        if (Arr == null || !Arr.Child.HasSubpath(subpath))
                        {
                            _context.Logger.LogError("Column name is invalid", path);
                            return;
                        }

                        node = Arr.Child;

                        // convert 0-base to 1-base
                        _indexes.Add(arrIndex + 1);
                    }
                }

                if (node.IndexType != null)
                {
                    object index = converter(node.IndexType, subpath);
                    _indexes.Add(index);
                }

                node = node.GetChild(subpath);
            }

            try
            {
                node.SetValue(row, _indexes.GetEnumerator(), converter(node.ValueType, value));
            }
            catch (Exception ex)
            {
                _context.Logger.LogError(ex, "Failed to convert value \"{CellValue}\" of type {PropertyType}", value, node.ValueType);
            }
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
        public IEnumerable<(Node, bool, List<object>)> TraverseLeaf()
        {
            _indexes.Clear();

            foreach (var node in Root.TraverseChildren(_indexes))
                yield return (node, false, _indexes);

            if (Arr != null)
            {
                foreach (var node in Arr.TraverseChildren(_indexes))
                    yield return (node, true, _indexes);
            }
        }
    }
}
