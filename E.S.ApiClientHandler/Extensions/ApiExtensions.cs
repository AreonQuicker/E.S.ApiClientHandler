using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace E.S.ApiClientHandler.Extensions
{
    public static class ApiExtensions
    {
        public static T To<T>(this JToken jToken, T defaultValue = null)
            where T : class
        {
            try
            {
                return jToken.ToObject<T>();
            }
            catch (Exception)
            {
                if (defaultValue != null) return defaultValue;

                return default;
            }
        }

        public static async Task<T> ToAsync<T>(this HttpResponseMessage response, T defaultValue = null,
            bool shouldBeSuccess = true)
            where T : class, new()
        {
            try
            {
                if (!response.IsSuccessStatusCode && shouldBeSuccess) return defaultValue;

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<T>(content);

                if (result is null
                    && typeof(T).IsGenericType
                    && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    return new T();

                return result ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static async Task<string> ToStringAsync(this HttpResponseMessage response, bool shouldBeSuccess = true)
        {
            try
            {
                if (!response.IsSuccessStatusCode && shouldBeSuccess) return default;

                var content = await response.Content.ReadAsStringAsync();

                return content;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static async Task<byte[]> ToBytesAsync(this HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode) return Array.Empty<byte>();

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }
    }
}