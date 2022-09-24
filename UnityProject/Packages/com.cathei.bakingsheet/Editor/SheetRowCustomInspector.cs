using System;
using System.IO;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cathei.BakingSheet.Editor
{
    [CustomEditor(typeof(SheetRowScriptableObject), true)]
    public class SheetRowCustomInspector : UnityEditor.Editor
    {
        private SerializedProperty serializedRow;
        private SerializedProperty unityReferences;

        private void OnEnable()
        {
            serializedRow = serializedObject.FindProperty("serializedRow");
            unityReferences = serializedObject.FindProperty("references");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            var jObject = JObject.Parse(serializedRow.stringValue);

            ExpandIdField(inspector, jObject.Value<string>(nameof(ISheetRow.Id)));

            foreach (var pair in jObject)
            {
                if (pair.Key == nameof(ISheetRow.Id))
                    continue;

                ExpandJsonToken(inspector, pair.Key, pair.Value);
            }

            return inspector;
        }

        private void ExpandJsonToken(VisualElement parent, string label, JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    ExpandJsonObject(parent, label, (JObject)jToken);
                    break;

                case JTokenType.Array:
                    ExpandJsonArray(parent, label, (JArray)jToken);
                    break;

                default:
                    ExpandJsonValue(parent, label, jToken);
                    break;
            }
        }

        private void ExpandJsonObject(VisualElement parent, string label, JObject jObject)
        {
            if (jObject.Count == 1 && jObject.TryGetValue("$ref", out var refToken))
            {
                ExpandUnityReference(parent, label, refToken);
                return;
            }

            var foldout = new Foldout
            {
                text = label
            };

            var box = new Box();

            parent.Add(foldout);
            foldout.Add(box);

            foreach (var pair in jObject)
            {
                ExpandJsonToken(box, pair.Key, pair.Value);
            }
        }

        private void ExpandJsonArray(VisualElement parent, string label, JArray jArray)
        {
            var foldout = new Foldout
            {
                text = label
            };

            parent.Add(foldout);

            for (int i = 0; i < jArray.Count; ++i)
            {
                ExpandJsonToken(foldout, $"Element {i}", jArray[i]);
            }
        }

        private void ExpandJsonValue(VisualElement parent, string label, JToken jToken)
        {
            var child = new TextField
            {
                label = label,
                value = jToken.Value<string>()
            };

            child.isReadOnly = true;

            parent.Add(child);
        }

        private void ExpandIdField(VisualElement parent, string value)
        {
            var child = new TextField
            {
                label = nameof(ISheetRow.Id),
                value = value
            };

            parent.Add(child);
        }

        private void ExpandUnityReference(VisualElement parent, string label, JToken jToken)
        {
            int refIndex = jToken.Value<int>();
            UnityEngine.Object refObj = null;

            if (0 <= refIndex && refIndex < unityReferences.arraySize)
                refObj = unityReferences.GetArrayElementAtIndex(refIndex).objectReferenceValue;

            var child = new ObjectField
            {
                label = label,
                value = refObj,
            };

            parent.Add(child);
        }
    }
}