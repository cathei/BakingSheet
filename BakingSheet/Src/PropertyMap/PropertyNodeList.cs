// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    public class PropertyNodeList : PropertyNode
    {
        private readonly PropertyNode _child;
        private int _maxCount = 1;

        private readonly bool _isVertical;

        public override Type IndexType => typeof(int);
        public override bool IsVertical => _isVertical;
        public override PropertyNode ColumnNode => _isVertical ? _child.ColumnNode : this;
        public override PropertyNode GetChild(string subpath) => _child;

        public PropertyNodeList(PropertyNode parent, string fullPath, Type valueType,
            GetterDelegate getter, SetterDelegate setter, PropertyInfo propertyInfo,
            ISheetContractResolver resolver, int depth, bool isVertical)
            : base(parent, fullPath, valueType, getter, setter, propertyInfo)
        {
            _isVertical = isVertical;
            _child = GenerateChildren(resolver, depth);
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

        public override IEnumerable<PropertyNode> TraverseChildren(List<object> indexes)
        {
            if (_isVertical)
            {
                // no need to loop through indexes for vertical list
                foreach (var node in _child.TraverseChildren(indexes))
                    yield return node;
                yield break;
            }

            int current = indexes.Count;

            // adding a slot
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

        private static bool ValueGetter(PropertyNode child, object obj, object key, out object value)
        {
            if (obj is IList list)
            {
                if (!(key is int idx))
                    throw new ArgumentException($"List index {key} is not an integer.", nameof(key));

                // convert 1-base to 0-base
                idx -= 1;

                if (idx < list.Count)
                {
                    value = list[idx];
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static void ValueSetter(PropertyNode child, object obj, object key, object value)
        {
            if (obj is IList list && key is int idx)
            {
                // convert 1-base to 0-base
                idx -= 1;

                while (list.Count <= idx)
                {
                    list.Add(child.ValueType.IsValueType ?
                        Activator.CreateInstance(child.ValueType) : null);
                }

                list[idx] = value;
            }
        }

        private PropertyNode GenerateChildren(ISheetContractResolver resolver, int depth)
        {
            var elementType = PropertyMap.GetGenericArgument(ValueType, typeof(IList<>))[0];

            var childPath = _isVertical ? FullPath : AppendIndex(depth);

            return Create(this, childPath, elementType, ValueGetter, ValueSetter,
                PropertyInfo, resolver, _isVertical ? depth : depth + 1);
        }
    }
}
