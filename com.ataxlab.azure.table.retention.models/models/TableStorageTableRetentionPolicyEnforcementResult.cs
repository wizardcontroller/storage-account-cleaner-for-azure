using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace com.ataxlab.azure.table.retention.models
{
    /// <summary>
    /// enable reporting on what happened after policy is aplied
    /// </summary>
    public class TableStorageTableRetentionPolicyEnforcementResult
    {
        public TableStorageTableRetentionPolicyEnforcementResult()
        {
            Policy = new TableStorageTableRetentionPolicy();
        }

        [JsonIgnore]
        [Key]
        public Guid Id { get; set; }

        public TableStorageTableRetentionPolicy Policy { get; set; }

        public int  PolicyTriggerCount { get; set; }
    }

    /// <summary>
    /// enable reporting on what happened after policy is aplied
    /// </summary>
    public class TableStorageEntityRetentionPolicyEnforcementResult
    {
        public TableStorageEntityRetentionPolicyEnforcementResult()
        {
            Policy = new TableStorageEntityRetentionPolicy();
        }


        [JsonIgnore]
        [Key]
        public Guid Id { get; set; }
        public TableStorageEntityRetentionPolicy Policy { get; set; }

        public int PolicyTriggerCount { get; set; }

    }

}
