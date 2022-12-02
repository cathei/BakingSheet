// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cathei.BakingSheet.Internal
{
    // property map for a sheet
    public partial class PropertyMap
    {
        public abstract class Node
        {
            public delegate bool GetterDelegate(Node child, object obj, object key, out object value);
            public delegate void SetterDelegate(Node child, object obj, object key, object value);
            public delegate IEnumerable<Attribute> AttributesGetterDelegate(Type attribute);
            public delegate object ModifyDelegate(object original);

            public Node Parent { get; set; }
            public string FullPath { get; set; }

            public Type IndexType { get; set; }
            public Type ValueType { get; set; }

            public GetterDelegate Getter { get; set; }
            public SetterDelegate Setter { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public ISheetValueConverter ValueConverter { get; set; }

            public virtual bool IsLeaf => false;
            public virtual bool IsVertical => false;
            public abstract Node? GetChild(string subpath);
            public virtual bool HasSubpath(string subpath) => false;

            // can be used when parent and child shares same column name
            public virtual Node ColumnNode => this;

            public abstract void UpdateIndex(object obj);
            public abstract int CalculateDepth();
            public abstract IEnumerable<Node> TraverseChildren(List<object> indexes);
            public abstract void GenerateChildren(ISheetContractResolver resolver, int depth);

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

            public bool TryGetValue(ISheetRow row, int vindex, IEnumerator<object> indexer, out object value)
            {
                object obj = row;

                if (Parent != null)
                    obj = Parent.GetValue(row, vindex, indexer);

                if (obj == null)
                {
                    value = null;
                    return false;
                }

                object index = Parent?.GetChildIndex(vindex, indexer);
                return Getter(this, obj, index, out value);
            }

            public object GetValue(ISheetRow row, int vindex, IEnumerator<object> indexer)
            {
                TryGetValue(row, vindex, indexer, out var value);
                return value;
            }

            public void SetValue(ISheetRow row, int vindex, IEnumerator<object> indexer, object value)
            {
                ModifyValue(row, vindex, indexer, _ => value);
            }

            public void ModifyValue(ISheetRow row, int vindex, IEnumerator<object> indexer, ModifyDelegate modifier)
            {
                if (Parent == null)
                {
                    Getter(this, row, null, out var obj);
                    modifier(obj);

                    // there would be no setter for root node
                    return;
                }

                Parent.ModifyValue(row, vindex, indexer, parentObj =>
                {
                    if (parentObj == null)
                        return null;

                    object index = Parent?.GetChildIndex(vindex, indexer);
                    Getter(this, parentObj, index, out var obj);

                    // for leaf nodes there might be no default constructor available
                    if (obj == null && !IsLeaf)
                        obj = Activator.CreateInstance(ValueType);

                    obj = modifier(obj);

                    Setter(this, parentObj, index, obj);

                    return parentObj;
                });
            }
        }
    }
}
