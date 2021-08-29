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
       public static string HashToSha256(params String[] values)
        {
            StringBuilder combined = new StringBuilder(string.Empty);
            foreach (var item in values)
            {
                combined.Append(item);
            }

            return combined.ToString().HashToGuid().ToString();
        }



        public static string HashToSha256(this String plaintext)
        {
            string ret = string.Empty;
            using (SHA256 sha256Hash = SHA256.Create())
            {

                
                byte[] sha267bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
                ret = HashToGuid(ConvertStringToHex(plaintext, Encoding.UTF8).ToString()).ToString();
                // a durable function entity key cannot contain special characters
                // return ConvertStringToHex(System.Convert.ToBase64String(sha267bytes), Encoding.UTF8);
            }

            return ret;
        }

        public static string ConvertStringToHex(String input, System.Text.Encoding encoding)
        {
            Byte[] stringBytes = encoding.GetBytes(input);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            return sbBytes.ToString();
        }

        public static Guid HashToGuid(params String[] values)
        {
            StringBuilder combined = new StringBuilder(string.Empty);
            foreach (var item in values)
            {
                combined.Append(item);
            }

            return combined.ToString().HashToGuid();
        }

        public static Guid HashToGuid(this String plaintext)
        {
            byte[] hashed;
            // Create a SHA256   


                var t = JsonConvert.SerializeObject(plaintext);

                var bytes = ASCIIEncoding.ASCII.GetBytes(plaintext);

                MD5 hashIt = MD5.Create();
                hashed = hashIt.ComputeHash(bytes);
         
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
