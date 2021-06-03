using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.models.azuremanagement
{

    public class ScaffoldingStorageAccount 
    {

        /// <summary>
        /// which user retrieved these results
        /// </summary>
        public string RequestingAzureAdUserOid { get; set; }

        /// <summary>
        /// the user wants to operate against this storage account
        /// </summary>
        [Required]
        public bool IsSelected { get; set; }
        /// <summary>
        /// v2 or what
        /// </summary>
        public string StorageAccountKind { get; set; }
        [ScaffoldColumn(false)]
        [Key]
        public string Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string StorageAccountType { get; set; }

        public string SkuName { get; set; }
        public string TenantId { get; set; }
    }

    public class StorageAccountDTO
    {
        public StorageAccountDTO()
        { }

        /// <summary>
        /// which user retrieved these results
        /// </summary>
        public String RequestingAzureAdUserOid { get; set; }

        /// <summary>
        /// the user wants to operate against this storage account
        /// </summary>
        [Required]
        public bool IsSelected { get; set; }
        /// <summary>
        /// v2 or what
        /// </summary>
        public String StorageAccountKind { get; set; }
        [ScaffoldColumn(false)]
        [Key]
        public String Id { get; set; }
        public string Location { get; set; }
        public String Name { get; set; }
        public String StorageAccountType { get; set; }

        public String SkuName { get; set; }
        public String TenantId { get; set; }
        public string SubscriptionId { get; set; }
    }
}
