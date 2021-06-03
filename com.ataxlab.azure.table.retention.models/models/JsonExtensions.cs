using Dynamitey.DynamicObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.models.extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// call this not from an orchestration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<T> FromJSONStringAsync<T>(this string value)
        {
            var ret = JsonConvert.DeserializeObject<T>(value);
            return await Task.FromResult<T>(ret);
        }

        /// <summary>
        /// call this not from an orchestration
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<string> ToJSONStringAsync(this object value)
        {
            var ret = JsonConvert.SerializeObject(value);

            return await Task.FromResult<string>(ret);
        }

        public static string ToJSONString(this object value)
        {
            var ret = JsonConvert.SerializeObject(value);

            return ret;
        }
    }
}
