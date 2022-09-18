// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Reflection;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Cathei.BakingSheet
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

            if (typeof(ISheetReference).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOReferenceConverter();
                return contract;
            }

            if (typeof(ISheetAssetPath).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetSOAssetPathConverter();
                return contract;
            }

            if (objectType.IsEnum)
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
    }
}

