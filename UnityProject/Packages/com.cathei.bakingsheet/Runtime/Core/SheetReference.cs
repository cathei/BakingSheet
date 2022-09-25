// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public interface ISheetReference
    {
        void Map(SheetConvertingContext context, ISheet sheet);

        object Id { get; set; }
        Type IdType { get; }

        ISheetRow Ref { get; set; }
    }

    public partial class Sheet<TKey, TValue>
    {
        /// <summary>
        /// Cross-sheet reference column to this Sheet.
        /// </summary>
        public sealed partial class Reference : ISheetReference
        {
            [Preserve]
            public TKey Id { get; private set; }
            [Preserve]
            public TValue Ref { get; private set; }

            object ISheetReference.Id
            {
                get => Id;
                set => Id = (TKey)value;
            }

            public Type IdType => typeof(TKey);

            ISheetRow ISheetReference.Ref
            {
                get => Ref;
                set => Ref = (TValue)value;
            }

            [Preserve]
            public Reference() { }

            public Reference(TKey id)
            {
                Id = id;
                Ref = null;
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
        }
    }

    public static class SheetReferenceExtensions
    {
        public static bool IsValid(this ISheetReference reference)
        {
            return reference?.Ref != null;
        }
    }
}
