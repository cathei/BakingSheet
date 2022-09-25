// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Unity;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cathei.BakingSheet.Editor
{
    [CustomEditor(typeof(SheetRowScriptableObject), true)]
    public class SheetRowCustomInspector : UnityEditor.Editor
    {
        private SerializedProperty serializedRow;
        private SerializedProperty unityReferences;
        private StyleSheet styleSheet;

        private void OnEnable()
        {
            serializedRow = serializedObject.FindProperty("serializedRow");
            unityReferences = serializedObject.FindProperty("references");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.cathei.bakingsheet/Editor/StyleSheet.uss");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            inspector.styleSheets.Add(styleSheet);

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
            if (jObject.TryGetValue("$type", out var metaType))
            {
                switch (metaType.Value<string>())
                {
                    case SheetMetaType.UnityObject:
                        ExpandUnityReference(parent, label, jObject.GetValue("Value"));
                        return;
                }
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
                if (pair.Key.StartsWith("$"))
                    continue;

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
                value = jToken.Value<string>(),
                isReadOnly = true
            };

            child.AddToClassList("readonly");

            parent.Add(child);
        }

        private void ExpandIdField(VisualElement parent, string value)
        {
            var child = new TextField
            {
                label = nameof(ISheetRow.Id),
                value = value,
                isReadOnly = true
            };

            child.AddToClassList("readonly");

            parent.Add(child);

            // var button = new Button
            // {
            //     text = "Change"
            // };
            //
            // button.RegisterCallback<ClickEvent, TextField>(HandleNameChange, child);
            //
            // child.Add(button);
        }

        // private void HandleNameChange(ClickEvent evt, TextField child)
        // {
        //     serializedObject.targetObject.name = child.text;
        //     serializedObject.ApplyModifiedProperties();
        //     AssetDatabase.SaveAssets();
        // }

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

            child.AddToClassList("readonly");
            parent.Add(child);
        }
    }
}