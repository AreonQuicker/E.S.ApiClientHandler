using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Models;

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
                if (defaultValue != null)
                {
                    return defaultValue;
                }

                return default(T);
            }
        }

        public static async Task<T> ToAsync<T>(this HttpResponseMessage response, T defaultValue = null, bool shouldBeSuccess = true)
            where T : class, new()
        {
            try
            {
                if (!response.IsSuccessStatusCode && shouldBeSuccess)
                {
                    if (defaultValue != null)
                    {
                        return defaultValue;
                    }

                    return default(T);
                }

                string content = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<T>(content);

                if (result is null
                    && typeof(T).IsGenericType
                    && typeof(T).GetGenericTypeDefinition() != null
                    && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    return new T();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }

                return default(T);
            }
        }

        public static async Task<string> ToStringAsync(this HttpResponseMessage response, bool shouldBeSuccess = true)
        {
            try
            {
                if (!response.IsSuccessStatusCode && shouldBeSuccess)
                {               
                    return default(string);
                }

                string content = await response.Content.ReadAsStringAsync();            

                return content;
            }
            catch (Exception)
            {
                return default(string);
            }
        }
        public static async Task<byte[]> ToBytesAsync(this HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<byte>();
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }
    }
}
