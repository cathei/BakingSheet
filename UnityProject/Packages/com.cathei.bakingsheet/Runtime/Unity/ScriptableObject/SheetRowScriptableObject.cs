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
        private string _serializedRow;

        [SerializeField]
        private List<UnityEngine.Object> _references = new List<UnityEngine.Object>();

        private ISheetRow _row;

        public T GetRow<T>() where T : ISheetRow
        {
            if (_row != null)
                return (T)_row;

            var row = DeserializeRow<T>(_serializedRow, _references);
            _row = row;
            return row;
        }

        public T SetRow<T>(T row) where T : ISheetRow
        {
            _row = row;
            return row;
        }

        public void OnBeforeSerialize()
        {
            if (_row == null)
                return;

            _serializedRow = SerializeRow(_row, _references);
        }

        public void OnAfterDeserialize()
        {
            _row = null;
        }

        protected abstract string SerializeRow(ISheetRow row, List<UnityEngine.Object> references);
        protected abstract T DeserializeRow<T>(string serializedRow, List<UnityEngine.Object> references);
    }
}
