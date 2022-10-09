// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
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

            public override int CalculateDepth()
            {
                return _child.CalculateDepth() + (_isVertical ? 0 : 1);
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

            private static bool ValueGetter(Node child, object obj, object key, out object value)
            {
                if (obj is IList list)
                {
                    // convert 1-base to 0-base
                    int idx = (int)key - 1;

                    if (idx < list.Count)
                    {
                        value = list[idx];
                        return true;
                    }
                }

                value = null;
                return false;
            }

            private static void ValueSetter(Node child, object obj, object key, object value)
            {
                if (obj is IList list)
                {
                    // convert 1-base to 0-base
                    int idx = (int)key - 1;

                    while (list.Count <= idx)
                    {
                        list.Add(child.ValueType.IsValueType ?
                            Activator.CreateInstance(child.ValueType) : null);
                    }

                    list[idx] = value;
                }
            }

            public override void GenerateChildren(ISheetContractResolver resolver, int depth)
            {
                var elementType = GetGenericArgument(ValueType, typeof(IList<>))[0];

                IndexType = typeof(int);

                var child = CreateNode(elementType);

                child.Parent = this;
                child.ValueType = elementType;
                child.FullPath = _isVertical ? FullPath : AppendIndex(depth);

                child.Getter = ValueGetter;
                child.Setter = ValueSetter;
                child.PropertyInfo = PropertyInfo;

                child.GenerateChildren(resolver, _isVertical ? depth : depth + 1);

                _child = child;
            }
        }
    }
}
