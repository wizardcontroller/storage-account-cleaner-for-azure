using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using com.ataxlab.azure.table.retention.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using com.ataxlab.azure.table.retention.state.entities;
using Newtonsoft.Json.Serialization;

namespace com.ataxlab.functions.table.retention.entities
{
    /// <summary>
    /// container for job input and output
    /// per storage account
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class RetentionPolicyTupleContainerEntity
    {
        public RetentionPolicyTupleContainerEntity()
        {
            Id = Guid.NewGuid();
        }

        [ScaffoldColumn(false)]
        [Key]
        public Guid Id { get; set; }

        [ScaffoldColumn(false)]
        [NotMapped]
        [JsonProperty("RetentionPolicyTuples")]

        /*
         * wraps storage accounts and policy wrappers
         */
        internal Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity> SourceTuple { get; set; }
        
        /// <summary>
        /// wraps Table retention and Entitty retention policies
        /// </summary>
        public TableStorageRetentionPolicyEntity TableStorageRetentionPolicy { get; set; }
        public StorageAccountEntity StorageAccount { get; set; }


        public TableStorageTableRetentionPolicyEnforcementResultEntity TableStoragePolicyEnforcementResult { get; set; }

        public TableStorageEntityRetentionPolicyEnforcementResultEntity TableStorageEntityPolicyEnforcementResult { get; set; }
    }

}
