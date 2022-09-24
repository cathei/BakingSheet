// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cathei.BakingSheet
{
    public abstract class SheetRowScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string serializedRow;

        [SerializeField]
        private List<UnityEngine.Object> references;

        private ISheetRow _row;

        private void Reset()
        {
            serializedRow = null;
            references = new List<UnityEngine.Object>();
            _row = null;
        }

        public T GetRow<T>() where T : class, ISheetRow
        {
            if (_row != null)
                return _row as T;

            _row = DeserializeRow(typeof(T), serializedRow, references);
            return _row as T;
        }

        public void SetRow<T>(T row) where T : ISheetRow
        {
            _row = row;
        }

        public void OnBeforeSerialize()
        {
            if (_row == null)
                return;

            serializedRow = SerializeRow(_row, references);
        }

        public void OnAfterDeserialize()
        {
            _row = null;
        }

        protected abstract string SerializeRow(ISheetRow row, List<UnityEngine.Object> references);
        protected abstract ISheetRow DeserializeRow(Type type, string serializedRow, List<UnityEngine.Object> references);
    }
}
