// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
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

            public override int CalculateDepth()
            {
                return _child.CalculateDepth() + 1;
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

            private static object ValueGetter(Node child, object obj, object key)
            {
                if (obj is IDictionary dict)
                    return dict[key];
                return null;
            }

            private static void ValueSetter(Node child, object obj, object key, object value)
            {
                if (obj is IDictionary dict)
                    dict[key] = value;
            }

            public override void GenerateChildren(ISheetContractResolver resolver, int depth)
            {
                var arguments = GetGenericArgument(ValueType, typeof(IDictionary<,>));
                var keyType = arguments[0];
                var elementType = arguments[1];

                IndexType = keyType;

                var child = CreateNode(elementType);

                child.Parent = this;
                child.ValueType = elementType;
                child.FullPath = AppendIndex(depth);

                child.Getter = ValueGetter;
                child.Setter = ValueSetter;
                child.PropertyInfo = PropertyInfo;

                child.GenerateChildren(resolver, depth + 1);

                _child = child;
            }
        }

        private const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

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
    }
}
