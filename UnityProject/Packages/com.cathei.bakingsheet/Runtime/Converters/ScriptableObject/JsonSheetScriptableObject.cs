// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class JsonSheetScriptableObject : SheetScriptableObject
    {
        private static JsonSerializerSettings _settings;

        protected virtual JsonSerializerSettings GetSettings()
        {
            if (_settings != null)
                return _settings;

            _settings = new JsonSerializerSettings();

            _settings.Error = (_, err) =>
                JsonSheetConverter.ErrorHandler(UnityLogger.Default, err);

            _settings.ContractResolver = new JsonSheetContractResolver();

            _settings.Converters.Add(new StringEnumConverter());
            _settings.Converters.Add(new SheetReferenceConverter());
            _settings.Converters.Add(new JsonSheetScriptableObjectConverter());

            return _settings;
        }

        protected override string SerializeRow(ISheetRow row, List<UnityEngine.Object> references)
        {
            var settings = GetSettings();
            return JsonConvert.SerializeObject(row, settings);
        }

        protected override T DeserializeRow<T>(string serializedRow, List<UnityEngine.Object> references)
        {
            var settings = GetSettings();
            return JsonConvert.DeserializeObject<T>(serializedRow, settings);
        }
    }
}
