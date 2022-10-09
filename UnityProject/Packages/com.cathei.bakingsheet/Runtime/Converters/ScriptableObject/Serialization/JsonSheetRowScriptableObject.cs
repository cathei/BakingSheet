// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cathei.BakingSheet.Unity
{
    public class JsonSheetRowScriptableObject : SheetRowScriptableObject
    {
        private static JsonSerializerSettings GetSettings(List<UnityEngine.Object> references)
        {
            var settings = new JsonSerializerSettings
            {
                Error = JsonSheetSOContractResolver.ErrorHandler,
                ContractResolver = JsonSheetSOContractResolver.Instance,
            };

            settings.Converters.Add(new JsonSheetUnityObjectConverter(references));

            return settings;
        }

        protected override string SerializeRow(ISheetRow row, List<UnityEngine.Object> references)
        {
            // initial reference values
            references.Clear();

            var settings = GetSettings(references);
            return JsonConvert.SerializeObject(row, settings);
        }

        protected override ISheetRow DeserializeRow(Type type, string serializedRow, List<UnityEngine.Object> references)
        {
            if (!typeof(ISheetRow).IsAssignableFrom(type))
                return null;

            if (string.IsNullOrEmpty(serializedRow))
                return null;

            var settings = GetSettings(references);
            return (ISheetRow)JsonConvert.DeserializeObject(serializedRow, type, settings);

        }
    }
}
