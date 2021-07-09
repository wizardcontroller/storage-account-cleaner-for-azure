using com.ataxlab.azure.table.retention.state.entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.entities
{
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class ApplianceJobOutputEntity
    {
        public ApplianceJobOutputEntity()
        {
            // one per storage account
            retentionPolicyJobs = new List<RetentionPolicyTupleContainerEntity>();
            id = Guid.NewGuid();
        }


        public Guid id { get; set; }

        /// <summary>
        /// container for policies and enforcement results
        /// one per job
        /// </summary>
        public List<RetentionPolicyTupleContainerEntity> retentionPolicyJobs { get; set; }
    }
}
