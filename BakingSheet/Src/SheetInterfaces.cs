// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    public interface ISheet : IEnumerable
    {
        string Name { get; set; }
        Type RowType { get; }

        int Count { get; }
        bool Contains(object key);
        void Add(object value);

        new IEnumerator<ISheetRow> GetEnumerator();
        PropertyMap GetPropertyMap(SheetConvertingContext context);

        void MapReferences(SheetConvertingContext context, Dictionary<Type, ISheet> rowTypeToSheet);
        void PostLoad(SheetConvertingContext context);
        void VerifyAssets(SheetConvertingContext context);
    }

    public interface ISheet<in TKey, out TRow> : ISheet, IReadOnlyList<TRow>
        where TRow : ISheetRow
    {
        TRow this[TKey key] { get; }
        TRow Find(TKey key);

        bool Contains(TKey key);
        bool Remove(TKey key);

        new int Count { get; }
        new IEnumerator<TRow> GetEnumerator();
    }

    public interface ISheet<out TRow> : ISheet<string, TRow>
        where TRow : ISheetRow
    { }

    public interface ISheetRow
    {
        object Id { get; }
        int Index { get; }
    }

    public interface ISheetRow<out TKey> : ISheetRow
    {
        new TKey Id { get; }
    }

    public interface ISheetRowElem
    {
        int Index { get; }
    }

    public interface ISheetRowArray
    {
        IReadOnlyList<object> Arr { get; }
        Type ElemType { get; }
    }

    public interface ISheetRowArray<out TElem> : ISheetRowArray, IReadOnlyList<TElem> where TElem : ISheetRowElem
    {
        new IReadOnlyList<TElem> Arr { get; }
    }

    public interface ISheetFormatter
    {
        TimeZoneInfo TimeZoneInfo { get; }
        IFormatProvider FormatProvider { get; }
    }

    public interface ISheetImporter
    {
        Task<bool> Import(SheetConvertingContext context);
    }

    public interface ISheetExporter
    {
        Task<bool> Export(SheetConvertingContext context);
    }

    public interface ISheetConverter : ISheetImporter, ISheetExporter
    {

    }
}
