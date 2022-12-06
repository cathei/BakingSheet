// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
    }

    public class PropertyNodeDictionary : PropertyNode
    {
        private readonly PropertyNode _child;

        public override Type IndexType { get; }
        private HashSet<object> PossibleKeys { get; set; }

        public PropertyNodeDictionary(PropertyNode parent, string fullPath, Type valueType,
            GetterDelegate getter, SetterDelegate setter, PropertyInfo propertyInfo,
            ISheetContractResolver resolver, int depth)
            : base(parent, fullPath, valueType, getter, setter, propertyInfo)
        {
            var arguments = PropertyMap.GetGenericArgument(ValueType, typeof(IDictionary<,>));
            var keyType = arguments[0];
            var elementType = arguments[1];

            IndexType = keyType;
            _child = GenerateChildren(elementType, resolver, depth);
        }

        public override PropertyNode GetChild(string subpath) => _child;

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

        public override int CalculateDepth()
        {
            return _child.CalculateDepth() + 1;
        }

        public override IEnumerable<PropertyNode> TraverseChildren(List<object> indexes)
        {
            if (PossibleKeys == null)
                yield break;

            int current = indexes.Count;

            // increase count
            indexes.Add(null);

            foreach (var key in PossibleKeys)
            {
                indexes[current] = key;

                foreach (var node in _child.TraverseChildren(indexes))
                    yield return node;
            }

            indexes.RemoveAt(current);
        }

        private static bool ValueGetter(PropertyNode child, object obj, object key, out object value)
        {
            Debug.Assert(key != null);

            if (obj is IDictionary dict && dict.Contains(key))
            {
                value = dict[key];
                return true;
            }

            value = null;
            return false;
        }

        private static void ValueSetter(PropertyNode child, object obj, object key, object value)
        {
            Debug.Assert(key != null);

            if (obj is IDictionary dict)
                dict[key] = value;
        }

        private PropertyNode GenerateChildren(Type elementType, ISheetContractResolver resolver, int depth)
        {
            var childPath = AppendIndex(depth);
            return Create(this, childPath, elementType, ValueGetter, ValueSetter, PropertyInfo, resolver, depth + 1);
        }
    }
}
