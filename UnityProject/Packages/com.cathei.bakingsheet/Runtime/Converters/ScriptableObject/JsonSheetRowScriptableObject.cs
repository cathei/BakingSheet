// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet
{
    public class JsonSheetRowScriptableObject : SheetRowScriptableObject
    {
        public static void ErrorHandler(ILogger logError, ErrorEventArgs err)
        {
            if (err.ErrorContext.Member?.ToString() == nameof(ISheetRow.Id) &&
                err.ErrorContext.OriginalObject is ISheetRow &&
                !(err.CurrentObject is ISheet))
            {
                // if Id has error, the error must be handled on the sheet level
                return;
            }

            using (logError.BeginScope(err.ErrorContext.Path))
                logError.LogError(err.ErrorContext.Error, err.ErrorContext.Error.Message);

            err.ErrorContext.Handled = true;
        }

        protected virtual JsonSerializerSettings GetSettings(List<UnityEngine.Object> references)
        {
            var settings = new JsonSerializerSettings
            {
                Error = (_, err) => ErrorHandler(UnityLogger.Default, err),
                ContractResolver = JsonSheetSOContractResolver.Instance
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
