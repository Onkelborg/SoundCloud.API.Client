using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SoundCloud.API.Client.Internal.Infrastructure.Objects
{
    internal static class ParameterExtensions
    {
        internal static string GetParameterName<TEnum>(this TEnum value) where TEnum : struct
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(ParameterAttribute)) as ParameterAttribute;
            if (attribute == null)
            {
                throw new NotImplementedException(string.Format("ParameterAttribute not found for value {0}", value));
            }

            return attribute.ParameterName;
        }

        internal static string ToParameterValue<TIn>(this TIn? input, Func<TIn, string> convert, string defaultValue = null)
            where TIn : struct
        {
            return input.HasValue ? convert(input.Value) : defaultValue;
        }

        internal static void AddOrUpdate(this Dictionary<string, object> dictionary, string key, object value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        internal static void SafeRemove(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }
        }

        internal static Dictionary<string, object> SetPagination(this Dictionary<string, object> parameters, int offset, int limit)
        {
            parameters.AddOrUpdate("offset", offset);
            parameters.AddOrUpdate("limit", limit);

            return parameters;
        }

        internal static TEnum GetValue<TEnum>(this string parameterName)
        {
            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                var field = value.GetType().GetField(value.ToString());
                var attribute = Attribute.GetCustomAttribute(field, typeof(ParameterAttribute)) as ParameterAttribute;
                if (attribute == null)
                {
                    continue;
                }

                if (attribute.ParameterName == parameterName)
                {
                    return (TEnum) value;
                }
            }

            return default (TEnum);
        }

        public static Dictionary<string, object> GetDiff<T>(this T to, T @from) where T : class
        {
            var diff = new Dictionary<string, object>();

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (!property.PropertyType.IsPrimitive && !(property.PropertyType == typeof(string)) &&
                    (property.PropertyType != typeof(SoundCloud.API.Client.Internal.Objects.Track[])))
                {
                    continue;
                }

                var jsonProperty = property.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() as JsonPropertyAttribute;
                if (jsonProperty == null)
                {
                    continue;
                }

                var jsonIgnore = property.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault() as JsonIgnoreAttribute;
                if(jsonIgnore != null)
                {
                    continue;
                }

                var getMethod = property.GetGetMethod();
                if (getMethod == null)
                {
                    continue;
                }

                var fromValue = getMethod.Invoke(@from, new object[0]);
                var toValue = getMethod.Invoke(to, new object[0]);

                if (property.PropertyType != typeof(SoundCloud.API.Client.Internal.Objects.Track[]))
                {

                    if ((fromValue == null && toValue == null) || (fromValue != null && fromValue.Equals(toValue)))
                    {
                        continue;
                    }
                }
                else
                {
                    var add = true;
                    var fromArray = fromValue as SoundCloud.API.Client.Internal.Objects.Track[];
                    var toArray = toValue as SoundCloud.API.Client.Internal.Objects.Track[];
                    if(fromArray.Length == toArray.Length)
                    {
                        add = false;
                        for(var i=0; i < fromArray.Length; i++)
                        {
                            if(fromArray[i].Id != toArray[i].Id)
                            {
                                add = true;
                                break;
                            }
                        }
                    }
                    if(add)
                    {
                        diff.Add(
                            jsonProperty.PropertyName,
                            fromArray.Select(x => new
                            {
                                id = x.Id
                            }).ToArray());
                    }
                    continue;
                }

                diff.Add(jsonProperty.PropertyName, fromValue);
            }

            return diff;
        }

        internal static TOut? SafeGet<TIn, TOut>(this TIn? input, Func<TIn, TOut?> converter) 
            where TIn : struct
            where TOut : struct

        {
            return input.HasValue ? converter(input.Value) : default (TOut?);
        }
    }
}