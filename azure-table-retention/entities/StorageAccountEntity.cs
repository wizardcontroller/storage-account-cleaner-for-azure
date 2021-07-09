using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace com.ataxlab.azure.table.retention.state.entities
{

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class StorageAccountEntity
    {
        public StorageAccountEntity()
        { }

        /// <summary>
        /// which user retrieved these results
        /// </summary>

        [JsonProperty("requestingAzureAdUserOid")]
        public string RequestingAzureAdUserOid { get; set; }

        /// <summary>
        /// the user wants to operate against this storage account
        /// </summary>
        [Required]
        public bool IsSelected { get; set; }
        /// <summary>
        /// v2 or what
        /// </summary>

        [JsonProperty("storageAccountKind")]
        public string StorageAccountKind { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("storageAccountType")]
        public string StorageAccountType { get; set; }


        [JsonProperty("skuName")]
        public string SkuName { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// avoid passing this to activities
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Uri PrimaryTableStorageEndpoint { get; set; }
        
        /// <summary>
        /// do not pass this to activities 
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Key { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string ConnectionString { get
{
                return String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net", this.Name, this.Key);

            }
        }
    }
}
