// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    public class PropertyNodeObject : PropertyNode
    {
        private const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

        private Dictionary<string, PropertyNode>? _children;

        public override Type? IndexType => null;

#if !NETSTANDARD2_0
        [MemberNotNullWhen(false, nameof(_children))]
#endif
        protected override bool IsLeaf => _children == null;

        public PropertyNodeObject(PropertyNode? parent, string? fullPath, Type valueType,
            GetterDelegate getter, SetterDelegate? setter, PropertyInfo? propertyInfo,
            ISheetContractResolver resolver, int depth)
            : base(parent, fullPath, valueType, getter, setter, propertyInfo)
        {
            GenerateChildren(resolver, depth);
        }

        public override PropertyNode? GetChild(string subpath) => _children?[subpath];

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

            foreach (var child in _children!.Values)
            {
                child.Getter(child, obj, null, out var elem);
                if (elem != null)
                    child.UpdateIndex(elem);
            }
        }

        public override int CalculateDepth()
        {
            if (IsLeaf)
                return 0;

            int depth = 0;

            foreach (var child in _children!.Values)
                depth = Math.Max(depth, child.CalculateDepth());

            return depth;
        }

        public override IEnumerable<PropertyNode> TraverseChildren(List<object> indexes)
        {
            if (IsLeaf)
            {
                yield return this;
                yield break;
            }

            // Id column should come first
            if (_children!.TryGetValue(nameof(ISheetRow.Id), out var idChild))
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

        internal static bool ValueGetter(PropertyNode child, object obj, object? key, out object? value)
        {
            Debug.Assert(child.PropertyInfo != null);
            value = child.PropertyInfo!.GetValue(obj);
            return true;
        }

        private static void ValueSetter(PropertyNode child, object obj, object? key, object? value)
        {
            Debug.Assert(child.PropertyInfo != null);
            child.PropertyInfo!.SetValue(obj, value);
        }

        private void GenerateChildren(ISheetContractResolver resolver, int depth)
        {
            Debug.Assert(PropertyInfo != null);

            ValueConverter = resolver.GetValueConverter(PropertyInfo!) ??
                             resolver.GetValueConverter(ValueType);

            // leaf node (convertable)
            if (ValueConverter != null)
                return;

            _children = new Dictionary<string, PropertyNode>();

            foreach (PropertyInfo propertyInfo in ValueType.GetProperties(BindingFlag))
            {
                if (!ShouldInclude(propertyInfo))
                    continue;

                var childPath = AppendPath(propertyInfo.Name);
                var child = PropertyNode.Create(this, childPath, propertyInfo.PropertyType,
                    ValueGetter, ValueSetter, propertyInfo, resolver, depth);

                _children.Add(propertyInfo.Name, child);
            }
        }

        private static bool ShouldInclude(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsDefined(typeof(NonSerializedAttribute)))
                return false;

            if (propertyInfo.SetMethod == null)
                return false;

            return true;
        }
    }
}
