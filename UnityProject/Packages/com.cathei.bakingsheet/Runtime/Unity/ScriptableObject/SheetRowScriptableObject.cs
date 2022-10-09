// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cathei.BakingSheet.Unity
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

        internal T GetRow<T>() where T : class, ISheetRow
        {
            return GetRow(typeof(T)) as T;
        }

        internal ISheetRow GetRow(Type type)
        {
            if (_row != null)
                return _row;

            _row = DeserializeRow(type, serializedRow, references);
            return _row;
        }

        internal void SetRow<T>(T row) where T : ISheetRow
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
