using com.ataxlab.azure.table.retention.state.entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.entities
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ApplianceJobOutputEntity
    {
        public ApplianceJobOutputEntity()
        {
            // one per storage account
            RetentionPolicyJobs = new List<RetentionPolicyTupleContainerEntity>();
            id = Guid.NewGuid();
        }

        [JsonIgnore]
        public Guid id { get; set; }

        /// <summary>
        /// container for policies and enforcement results
        /// one per job
        /// </summary>
        public List<RetentionPolicyTupleContainerEntity> RetentionPolicyJobs { get; set; }
    }
}
