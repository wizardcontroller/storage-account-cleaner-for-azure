using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.models.models.auth
{
    /// <summary>
    /// represents a compound key identifying 
    /// - user
    /// - subscription
    /// - storage account
    /// </summary>

    public class ApplianceSessionContext
    {
        public ApplianceSessionContext()
        {
            AvailableSubscriptions = new List<SubscriptionDTO>();
            AvailableStorageAccounts = new List<StorageAccountDTO>();
            SelectedStorageAccounts = new List<StorageAccountDTO>();
            JobOutput = new List<ApplianceJobOutput>();
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// represents a hash of all the other properties
        /// </summary>
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        public string TenantId { get; set; }

        [JsonProperty("SelectedSubscriptionId")]
        public string SelectedSubscriptionId { get; set; }

        public SubscriptionDTO SelectedSubscription { get; set; }

        public string SelectedStorageAccountId { get; set; }

        [JsonProperty("SelectedStorageAccounts")]
        public List<StorageAccountDTO> SelectedStorageAccounts { get; set; }

        [JsonProperty("UserOid")]
        public string UserOid { get; set; }

        public List<SubscriptionDTO> AvailableSubscriptions { get; set; }
        public List<StorageAccountDTO> AvailableStorageAccounts { get; set; }

        public List<ApplianceJobOutput> JobOutput { get; set; }

        [JsonProperty("CurrentJobOutput")]
        public ApplianceJobOutput CurrentJobOutput { get; set; }
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class JobOutputLogEntry
    {
        public DateTime timeStamp { get; set; }

        public string summary { get; set; }

        public string detail { get; set; }

        public string severity { get; set; }

        public string source { get; set; }

        public AvailableCommand ExecutedCommand { get; set; }
    }


    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class JobOutputLogEntity
    {
        public JobOutputLogEntity()
        {
            this.logEntries = new List<JobOutputLogEntry>();
        }

        public string userOid { get; set; }
        public string userTenantId { get; set; }

        public int rowCount { get; set; }

        public List<JobOutputLogEntry> logEntries { get; set; }

    }

    public class ApplianceJobOutput
    {
        public ApplianceJobOutput()
        {
            RetentionPolicyJobs = new List<RetentionPolicyTupleContainer>();
        }

        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        /// <summary>
        /// useful for rendering asp.net core display templates from a collection
        /// </summary>
        internal List<Tuple<TableStorageTableRetentionPolicy, RetentionPolicyTupleContainer>> TableRetentionPolicies
        {
            get
            {
                var ret = new List<Tuple<TableStorageTableRetentionPolicy, RetentionPolicyTupleContainer>>();
                foreach (var job in RetentionPolicyJobs)
                {
                    ret.Add(new Tuple<TableStorageTableRetentionPolicy, RetentionPolicyTupleContainer>
                                (job.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy, job));
                }

                return ret;
            }
        }

        #region obsolete 
        [Obsolete]
        public TableStorageTableRetentionPolicyEnforcementResult TableRetentionResult { get; set; }

        [Obsolete]
        public TableStorageEntityRetentionPolicyEnforcementResult TableEntityRetentionResult { get; set; }

        [Obsolete]
        public TableStorageEntityRetentionPolicy TableEntityRetentionPolicy { get; set; }

        [Obsolete]
        public TableStorageTableRetentionPolicy TableRetentionPolicy { get; set; }

        [Obsolete]
        [ScaffoldColumn(false)]
        [NotMapped]
        [JsonProperty("RetentionPolicyTuples")]
        internal List<Tuple<TableStorageRetentionPolicy, StorageAccountDTO>> PolicyTuples { get; set; }

        [Obsolete]
        [JsonIgnore]
        [JsonProperty("NotTheWireRetentionPolicyTuples")]
        public List<RetentionPolicyTupleContainer> RetentionPolicyTuples
        {
            get
            {
                var ret = new List<RetentionPolicyTupleContainer>();
                if (PolicyTuples != null)
                {
                    foreach (var tuple in PolicyTuples)
                    {
                        ret.Add(new RetentionPolicyTupleContainer()
                        {
                            SourceTuple = tuple,
                            TableStorageRetentionPolicy = tuple.Item1,
                            StorageAccount = tuple.Item2
                        });
                    }
                }
                return ret;
            }
        }

        #endregion obsolete 
        /// <summary>
        /// container for policies and enforcement results
        /// one per job
        /// </summary>
        public List<RetentionPolicyTupleContainer> RetentionPolicyJobs { get; set; }
    }

    public class RetentionPolicyTupleContainer
    {
        public RetentionPolicyTupleContainer()
        {

        }


        [ScaffoldColumn(false)]
        [Key]
        public Guid Id { get; set; }


        /// <summary>
        /// tuples interfere with asp.net core mvc view scaffolding
        /// </summary>
        [ScaffoldColumn(false)]
        [NotMapped]
        [JsonProperty("RetentionPolicyTuples")]
        internal Tuple<TableStorageRetentionPolicy, StorageAccountDTO> SourceTuple { get; set; }

        /// <summary>
        /// tuples interfere with asp.net core mvc view scaffolding
        /// </summary>
        public async Task<Tuple<TableStorageRetentionPolicy, StorageAccountDTO>> GetSourceTuple()
        {
            return await Task.FromResult(SourceTuple);
        }
        /// <summary>
        /// thisproperty is a container for policies that generate results
        /// surfaced as properties on this object
        /// </summary>
        public TableStorageRetentionPolicy TableStorageRetentionPolicy { get; set; }

        public StorageAccountDTO StorageAccount { get; set; }

        public TableStorageTableRetentionPolicyEnforcementResult TableStoragePolicyEnforcementResult { get; set; }

        public TableStorageEntityRetentionPolicyEnforcementResult TableStorageEntityPolicyEnforcementResult { get; set; }
    }
}
