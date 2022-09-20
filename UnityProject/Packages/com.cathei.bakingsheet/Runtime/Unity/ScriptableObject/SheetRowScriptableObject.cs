// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public abstract class SheetRowScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        // type metadata that used for editor type check
        [SerializeField]
        private string _typeMeta;
#endif

        [SerializeField]
        private string serializedRow;

        [SerializeField]
        private List<UnityEngine.Object> references = new List<UnityEngine.Object>();

        private ISheetRow _row;

        public T GetRow<T>() where T : class, ISheetRow
        {
            if (_row != null)
                return _row as T;

            _row = DeserializeRow(typeof(T), serializedRow, references);
            return _row as T;
        }

        public ISheetRow GetRow(Type type)
        {
            if (_row != null)
                return _row;

            return _row = DeserializeRow(type, serializedRow, references);
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
