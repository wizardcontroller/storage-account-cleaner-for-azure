using com.ataxlab.azure.table.retention.state.entities;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.entities
{

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class JobOutputLogEntry
    {

        public DateTime timeStamp { get; set; }

        public string summary { get; set; }

        public string detail { get; set; }

        public string severity { get; set; }

        public string source { get; set; }

        public AvailableCommandEntity ExecutedCommand { get; set; }
    }

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
