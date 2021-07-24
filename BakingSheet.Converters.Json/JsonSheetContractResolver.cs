using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cathei.BakingSheet
{
    internal class JsonSheetContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(System.Type objectType)
        {
            if (typeof(ISheetRow).IsAssignableFrom(objectType) ||
                typeof(SheetRowElem).IsAssignableFrom(objectType))
            {
                return CreateObjectContract(objectType);
            }

            return base.CreateContract(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo pi)
            {
                var nonSerialize = pi.GetCustomAttribute<NonSerializedAttribute>() != null;
                var hasSetMethod = pi.SetMethod != null;

                property.Writable = !nonSerialize && hasSetMethod;
                property.ShouldSerialize = property.ShouldDeserialize = _ => !nonSerialize && hasSetMethod;
            }

            return property;
        }
    }
}
