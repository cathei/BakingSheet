using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Cathei.BakingSheet
{
    public static class SheetUtility
    {
        public static void ConvertFromRaw(SheetConvertingContext context, object obj, Dictionary<string, string> row)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

            var parentTag = context.Tag;

            foreach (var item in row)
            {
                var prop = type.GetProperty(item.Key, bindingFlags);
                if (prop == null)
                    continue;

                context.SetTag(parentTag, item.Key);

                try
                {
                    object value = ConvertValue(prop.PropertyType, item.Value);
                    prop.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"[{context.Tag}] Failed to convert value \"{item.Value}\" of type {prop.PropertyType}\n{ex}");
                }
            }
        }

        public static object ConvertValue(Type type, string value)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            
            if (typeof(ISheetReference).IsAssignableFrom(type))
            {
                var targetType = type.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISheetReference<>))
                    .Select(x => x.GetGenericArguments()[0])
                    .First();

                return Activator.CreateInstance(type, ConvertValue(targetType, value));
            }

            if(typeof(DateTime).IsAssignableFrom(type))
            {
                // TODO timezone support
                return DateTime.Parse(value);
            }

            if(typeof(TimeSpan).IsAssignableFrom(type))
            {
                return TimeSpan.Parse(value);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                var underlyingType = Nullable.GetUnderlyingType(type);
                return ConvertValue(underlyingType, value);
            }

            return Convert.ChangeType(value, type);
        }

        public static void MapReferences(SheetConvertingContext context, object obj)
        {
            var refProps = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => typeof(ISheetReference).IsAssignableFrom(p.PropertyType));

            var parentTag = context.Tag;

            foreach (var prop in refProps)
            {
                context.SetTag(parentTag, prop.Name);

                var refer = prop.GetValue(obj) as ISheetReference;
                refer.Map(context);
                prop.SetValue(obj, refer);
            }
        }

        public static void VerifyAssets(SheetConvertingContext context, object obj)
        {
            var assetProps = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetCustomAttribute(typeof(SheetAssetAttribute)) != null);

            var parentTag = context.Tag;
            
            foreach (var prop in assetProps)
            {
                context.SetTag(parentTag, prop.Name);

                foreach (var verifier in context.Verifiers)
                {
                    foreach (var att in prop.GetCustomAttributes(verifier.TargetAttribute))
                    {
                        var err = verifier.Verify(att);
                        if (err != null)
                            context.Logger.LogError($"[{context.Tag}] Verification: {err}");
                    }
                }
            }
        }

        internal class ContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (member is PropertyInfo pi)
                {
                    var hasSetMethod = pi.GetSetMethod(true) != null;

                    property.Writable = hasSetMethod;
                    property.ShouldSerialize = property.ShouldDeserialize = _ => hasSetMethod;
                }

                return property;
            }
        }

        internal class SheetReferenceConverter : JsonConverter<ISheetReference>
        {
            public override ISheetReference ReadJson(JsonReader reader, Type objectType, ISheetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                existingValue.ReadJson(reader, serializer);
                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, ISheetReference value, JsonSerializer serializer)
            {
                value.WriteJson(writer, serializer);
            }
        }

        public static JsonSerializerSettings GetSettings(ILogger logError)
        {
            var settings = new JsonSerializerSettings();

            settings.Error = (sender, err) =>
            {
                logError.LogError(err.ErrorContext.Error, err.ErrorContext.Error.Message);
                err.ErrorContext.Handled = true;
            };

            settings.ContractResolver = new ContractResolver();
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new SheetReferenceConverter());

            return settings;
        }

        public static string Serialize<T>(T obj, ILogger logger)
        {
            return JsonConvert.SerializeObject(obj, GetSettings(logger));
        }

        public static T Deserialize<T>(string json, ILogger logger)
        {
            return JsonConvert.DeserializeObject<T>(json, GetSettings(logger));
        }
    }
}
