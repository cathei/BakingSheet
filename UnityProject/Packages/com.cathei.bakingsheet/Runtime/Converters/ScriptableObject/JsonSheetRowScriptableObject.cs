// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class JsonSheetRowScriptableObject : SheetRowScriptableObject
    {
        protected virtual JsonSerializerSettings GetSettings(List<UnityEngine.Object> references)
        {
            var settings = new JsonSerializerSettings
            {
                Error = (_, err) =>
                    JsonSheetConverter.ErrorHandler(UnityLogger.Default, err),
                ContractResolver = JsonSheetScriptableObjectContractResolver.Instance
            };

            settings.Converters.Add(new JsonSheetUnityObjectConverter(references));

            return settings;
        }

        protected override string SerializeRow(ISheetRow row, List<UnityEngine.Object> references)
        {
            var settings = GetSettings(references);
            return JsonConvert.SerializeObject(row, settings);
        }

        protected override T DeserializeRow<T>(string serializedRow, List<UnityEngine.Object> references)
        {
            // initial reference values
            references.Clear();

            var settings = GetSettings(references);
            return JsonConvert.DeserializeObject<T>(serializedRow, settings);
        }
    }
}
