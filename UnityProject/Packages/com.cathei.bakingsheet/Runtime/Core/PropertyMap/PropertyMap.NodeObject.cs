// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
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
                    child.Getter(child, obj, null, out var elem);
                    child.UpdateIndex(elem);
                }
            }

            public override int CalculateDepth()
            {
                if (IsLeaf)
                    return 0;

                int depth = 0;

                foreach (var child in _children.Values)
                    depth = Math.Max(depth, child.CalculateDepth());

                return depth;
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

            internal static bool ValueGetter(Node child, object obj, object key, out object value)
            {
                value = child.PropertyInfo.GetValue(obj);
                return true;
            }

            private static void ValueSetter(Node child, object obj, object key, object value)
            {
                child.PropertyInfo.SetValue(obj, value);
            }

            public override void GenerateChildren(ISheetContractResolver resolver, int depth)
            {
                ValueConverter = resolver.GetValueConverter(PropertyInfo) ??
                                 resolver.GetValueConverter(ValueType);

                // leaf node (convertable)
                if (ValueConverter != null)
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
                    child.Getter = ValueGetter;
                    child.Setter = ValueSetter;
                    child.PropertyInfo = propertyInfo;

                    child.GenerateChildren(resolver, depth);

                    _children.Add(propertyInfo.Name, child);
                }
            }
        }
    }
}
