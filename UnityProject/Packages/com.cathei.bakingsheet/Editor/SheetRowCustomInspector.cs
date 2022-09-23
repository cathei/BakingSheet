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
        public override VisualElement CreateInspectorGUI()
        {
            var serializedRow = serializedObject.FindProperty("serializedRow");

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
    }
}