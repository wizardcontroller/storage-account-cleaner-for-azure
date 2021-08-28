using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.utility
{
    public static class CrucialExtensions
    {
        public static Guid HashToGuid(params String[] values)
        {
            String combined = string.Empty;
            foreach(var item in values)
            {
                String.Concat(combined, item);
            }

            return combined.HashToGuid();
        }

        public static Guid HashToGuid(this String plaintext) 
        {

            // var t = JsonConvert.SerializeObject(plaintext);
            var bytes = ASCIIEncoding.ASCII.GetBytes(plaintext);

            MD5 hashIt = MD5.Create();
            byte[] hashed = new MD5CryptoServiceProvider().ComputeHash(bytes); // hashIt.ComputeHash(bytes);

            return new Guid(hashed);
        }

        /// <summary>
        /// will throw exceptions like you wouldn't believe
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="req"></param>
        /// <returns></returns>
        public static async Task<T> DeserializeHttpMessageBody<T>(HttpRequestMessage req) where  T : class
        {
            T t = default(T);

            try
            {
                t = await req.Content.ReadAsAsync<T>();
            }
            catch(Exception e)
            { }

            return t;
        }

        public const string OrchestrationRouteTemplate = "tableretentionworkflow/{functionName}/{instanceId}";
    }

 
}
