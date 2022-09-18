// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Reflection;
using Cathei.BakingSheet.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Cathei.BakingSheet
{
    public class JsonSheetScriptableObjectContractResolver : JsonSheetContractResolver
    {
        public new static readonly JsonSheetScriptableObjectContractResolver Instance
            = new JsonSheetScriptableObjectContractResolver();

        protected override JsonContract CreateContract(System.Type objectType)
        {
            if (typeof(ISheetScriptableObjectReference).IsAssignableFrom(objectType))
            {
                var contract = base.CreateContract(objectType);
                contract.Converter = new JsonSheetScriptableObjectConverter();
                return contract;
            }

            return base.CreateContract(objectType);
        }
    }
}

