using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace com.ataxlab.azure.table.retention.state.entities
{

    [JsonObject(MemberSerialization.OptOut)]
    public class StorageAccountEntity
    {
        public StorageAccountEntity()
        { }

        /// <summary>
        /// which user retrieved these results
        /// </summary>

        [JsonProperty("RequestingAzureAdUserOid")]
        public string RequestingAzureAdUserOid { get; set; }

        /// <summary>
        /// the user wants to operate against this storage account
        /// </summary>
        [Required]
        public bool IsSelected { get; set; }
        /// <summary>
        /// v2 or what
        /// </summary>

        [JsonProperty("StorageAccountKind")]
        public string StorageAccountKind { get; set; }

        [JsonProperty("SubscriptionId")]
        public string SubscriptionId { get; set; }

        public string Id { get; set; }

        [JsonProperty("Location")]
        public string Location { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("StorageAccountType")]
        public string StorageAccountType { get; set; }


        [JsonProperty("SkuName")]
        public string SkuName { get; set; }

        [JsonProperty("TenantId")]
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
