// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Cathei.BakingSheet.Internal;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public interface ISheetReference
    {
        void Map(SheetConvertingContext context, ISheet sheet);

        object? Id { get; set; }
        Type IdType { get; }

        ISheetRow? Ref { get; }

#if !NETSTANDARD2_0
        [MemberNotNullWhen(true, nameof(Id), nameof(Ref))]
#endif
        bool IsValid();
    }

    public partial class Sheet<TKey, TValue>
    {
        /// <summary>
        /// Cross-sheet reference column to this Sheet.
        /// </summary>
        public partial struct Reference : ISheetReference
        {
            [Preserve, AllowNull, MaybeNull] public TKey Id { get; private set; }

            [Preserve] private TValue? reference;

            [Preserve]
            public TValue? Ref
            {
                get
                {
                    EnsureLoadReference();
                    return reference;
                }
                private set => reference = value;
            }

            object? ISheetReference.Id
            {
                get => Id;
                set => Id = value == null ? default : (TKey)value;
            }

            public Type IdType => typeof(TKey);

            ISheetRow? ISheetReference.Ref => Ref;

            public Reference(TKey id) : this()
            {
                Id = id;
            }

            void ISheetReference.Map(SheetConvertingContext context, ISheet sheet)
            {
                EnsureLoadReference();

                if (Id == null)
                    return;

                if (sheet is ISheet<TKey, TValue> referSheet)
                {
                    var referValue = referSheet.Find(Id);

                    if (Ref == null)
                    {
                        Ref = referValue;
                    }
                    else if (Ref != referValue)
                    {
                        context.Logger.LogError("Found different reference than originally set for \"{ReferenceId}\"",
                            Id);
                    }
                }

                if (Id != null && Ref == null)
                {
                    context.Logger.LogError("Failed to find reference \"{ReferenceId}\" on {SheetName}", Id,
                        sheet.Name);
                }
            }

            [return: MaybeNull]
            public static implicit operator TKey(Reference origin)
            {
                return origin.Id;
            }

            public override bool Equals(object? obj)
            {
                if (!(obj is Reference other))
                    return false;

                if (Id == null)
                    return other.Id == null;

                return Id.Equals(other.Id);
            }

            public override int GetHashCode()
            {
                return Id == null ? 0 : Id.GetHashCode();
            }

            public override string? ToString()
            {
                return Id == null ? "(null)" : Id.ToString();
            }

#if !NETSTANDARD2_0
            [MemberNotNullWhen(true, nameof(Id), nameof(Ref))]
#endif
            public bool IsValid()
            {
                return Ref != null;
            }

            /// <summary>
            /// Each Engine counterpart implements this
            /// </summary>
            partial void EnsureLoadReference();
        }
    }
}
