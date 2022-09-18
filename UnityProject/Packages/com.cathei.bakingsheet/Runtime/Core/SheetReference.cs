// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public partial interface ISheetReference
    {
        void Map(SheetConvertingContext context, ISheet sheet);

        object Id { get; set; }
        Type IdType { get; }
    }

    public partial class Sheet<TKey, TValue>
    {
        /// <summary>
        /// Cross-sheet reference column to this Sheet.
        /// </summary>
        public partial struct Reference : ISheetReference
        {
            [Preserve]
            public TKey Id { get; private set; }
            [Preserve]
            public TValue Ref { get; private set; }

            object ISheetReference.Id
            {
                get { return Id; }
                set { Id = (TKey)value; }
            }

            public Type IdType => typeof(TKey);

            public Reference(TKey id) : this()
            {
                Id = id;
                Ref = default(TValue);
            }

            void ISheetReference.Map(SheetConvertingContext context, ISheet sheet)
            {
                if (sheet is ISheet<TKey, TValue> referSheet)
                {
                    Ref = referSheet[Id];
                }

                if (Id != null && Ref == null)
                {
                    context.Logger.LogError("Failed to find reference \"{ReferenceId}\" on {SheetName}", Id, sheet.Name);
                }
            }

            public static implicit operator TKey(Reference origin)
            {
                return origin.Id;
            }

            public override bool Equals(object obj)
            {
                return obj is Reference refer && this == refer;
            }

            public override int GetHashCode()
            {
                return Id == null ? 0 : Id.GetHashCode();
            }

            public override string ToString()
            {
                return Id == null ? "(null)" : Id.ToString();
            }

            public static bool operator ==(Reference x, Reference y)
            {
                if (x.Id == null || y.Id == null)
                    return x.Id == null && y.Id == null;

                return x.GetType() == y.GetType() && x.Id.Equals(y.Id);
            }

            public static bool operator !=(Reference x, Reference y)
            {
                return !(x == y);
            }
        }
    }
}
