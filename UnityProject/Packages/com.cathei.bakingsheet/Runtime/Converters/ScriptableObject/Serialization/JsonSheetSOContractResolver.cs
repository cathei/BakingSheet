// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Reflection;
using Cathei.BakingSheet.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Cathei.BakingSheet.Unity
{
    public class JsonSheetSOContractResolver : DefaultContractResolver
    {
        public static readonly JsonSheetSOContractResolver Instance =
            new JsonSheetSOContractResolver();

        protected override JsonContract CreateContract(System.Type objectType)
        {
            if (typeof(ISheetRow).IsAssignableFrom(objectType) ||
                typeof(ISheetRowElem).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            if (typeof(IUnitySheetReference).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOReferenceConverter();
                return contract;
            }

            if (typeof(IUnitySheetAssetPath).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOAssetPathConverter();
                return contract;
            }

            if (objectType.IsEnum || Nullable.GetUnderlyingType(objectType)?.IsEnum == true)
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new StringEnumConverter();
                return contract;
            }

            return base.CreateContract(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo pi)
            {
                var nonSerialize = pi.IsDefined(typeof(NonSerializedAttribute));
                var hasSetMethod = pi.SetMethod != null;

                property.Writable = !nonSerialize && hasSetMethod;
                property.ShouldSerialize = property.ShouldDeserialize = _ => !nonSerialize && hasSetMethod;
            }

            return property;
        }

        public static void ErrorHandler(object sender, ErrorEventArgs err)
        {
            var logError = UnityLogger.Default;

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

    }
}

