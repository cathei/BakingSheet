using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cathei.BakingSheet
{
    internal class JsonSheetContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo pi)
            {
                // var shouldIgnore = pi.
                var hasSetMethod = pi.GetSetMethod(true) != null;

                property.Writable = hasSetMethod;
                property.ShouldSerialize = property.ShouldDeserialize = _ => hasSetMethod;
            }

            return property;
        }
    }
}
