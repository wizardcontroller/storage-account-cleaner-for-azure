using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.models.azuremanagement
{

    public class TenantResponse
    {
        [Key]
        [JsonIgnore]
        public string id { get; set; }
        [JsonProperty("value")]
        public List<TenantDTO> Tenants { get; set; }

        [JsonProperty("count")]
        public CountDTO Count { get; set; }
    }

    public class TenantDTO
    {
        public string id { get; set; }
        public string tenantId { get; set; }
        public string displayName { get; set; }
        public string defaultDomain { get; set; }
    }
}
