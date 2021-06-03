using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.models.azuremanagement
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class SubscriptionPoliciesDTO
    {
        [Key]
        public string locationPlacementId { get; set; }
        public string quotaId { get; set; }
        public string spendingLimit { get; set; }
    }

    public class SubscriptionDTO
    {
        /// <summary>
        /// the user wants to operate against this subscription
        /// </summary>
        public bool IsSelected { get; set; }
        /// <summary>
        /// which user retrieved these results
        /// </summary>
        public string RequestingAzureAdUserOid { get; set; }
        [Key]
        public string id { get; set; }
        public string authorizationSource { get; set; }
        [NotMapped]
        public string[] managedByTenants { get; set; }
        public string subscriptionId { get; set; }
        public string tenantId { get; set; }
        public string displayName { get; set; }
        public string state { get; set; }
        public SubscriptionPoliciesDTO subscriptionPolicies { get; set; }
    }

    public class CountDTO
    {
        [Key]
        [JsonProperty("type")]
        public string SubscriptionType { get; set; }

        [JsonProperty("value")]
        public int Count { get; set; }
    }

    public class SubscriptionResponse
    {
        [Key]
        [JsonIgnore]
        public string id { get; set; }
        [JsonProperty("value")]
        public List<SubscriptionDTO> Subscriptions { get; set; }
        
        [JsonProperty("count")]
        public CountDTO Count { get; set; }
    }

}
