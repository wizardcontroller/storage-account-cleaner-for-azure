using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.control;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace com.ataxlab.functions.table.retention.entities
{

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class TableStorageEntityRetentionPolicyEnforcementResultEntity
    {
        public TableStorageEntityRetentionPolicyEnforcementResultEntity()
        {
            Policy = new TableStorageEntityRetentionPolicyEntity();
            Id = Guid.NewGuid();
        }


        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonProperty("policy")]
        public TableStorageEntityRetentionPolicyEntity Policy { get; set; }

        [JsonProperty("policyTriggerCount")]
        public int PolicyTriggerCount { get; set; }

    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class TableStorageTableRetentionPolicyEnforcementResultEntity
    {
        public TableStorageTableRetentionPolicyEnforcementResultEntity()
        {
            Policy = new TableStorageTableRetentionPolicyEntity();
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [JsonProperty("policy")]
        public TableStorageTableRetentionPolicyEntity Policy { get; set; }

        [JsonProperty("policyTriggerCount")]
        public int PolicyTriggerCount { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum policyEnforcementMode { WhatIf, ApplyPolicy }

    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class TableStorageTableRetentionPolicyEntity
    {
        public TableStorageTableRetentionPolicyEntity()
        {

            // means months of history
            DeleteOlderTablesThanCurrentMonthMinusThis = 12;
            policyEnforcementMode = policyEnforcementMode.WhatIf;
            Id = Guid.NewGuid();
            MetricRetentionSurface = new MetricRetentionSurfaceEntity();
            this.WADMetricsTableNamePrefix = "WADMetrics";
            WADMetricsTableNames = new List<string>();
        }

        /// <summary>
        /// rendered list of yyyyMM strings represented by the policy delete age
        /// </summary>
        [JsonProperty("wADMetricsTableNames")] 
        public List<String> WADMetricsTableNames { get; private set; }
        
        [Obsolete]
        public async Task<int> InitializeWADMetricsTableNames()
        {
            var tableNames = 0;

            // get a timespan representing the policy
            var today = DateTime.UtcNow;
            var startDate = today.AddMonths(-DeleteOlderTablesThanCurrentMonthMinusThis);
            var timeSpan = today - startDate;
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                var yearString = startDate.AddDays(day).ToString("yyyy");
                var monthString = startDate.AddDays(day).ToString("MM");
                var dayString = startDate.ToString("d2");
                foreach (var aggregationPrefix in MetricRetentionSurface.AggregationPrefixes)
                {
                    string tableName = $"{WADMetricsTableNamePrefix}{aggregationPrefix}P10DV2S{yearString}{monthString}{dayString}";
                    WADMetricsTableNames.Add($"{tableName}");


                    tableNames++;
                }


            }

            return await Task.FromResult<int>(tableNames);
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("wADMetricsTableNamePrefix")]
        public string WADMetricsTableNamePrefix { get; set; }

        [JsonProperty("metricRetentionSurface")]
        public MetricRetentionSurfaceEntity MetricRetentionSurface { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("policyEnforcementMode")]
        public policyEnforcementMode policyEnforcementMode { get; set; }

        /// <summary>
        /// defaults to 65536 just in case man
        /// </summary>
        [JsonProperty("deleteOlderTablesThanCurrentMonthMinusThis")]
        public int DeleteOlderTablesThanCurrentMonthMinusThis { get; set; }

        [JsonProperty("oldestRetainedTable")]
        public DateTime OldestRetainedTable {get; set;}

        [JsonProperty("mostRecentRetainedTable")]
        public DateTime MostRecentRetainedTable {get; set;}
        /// <summary>
        /// you'd be better off supplying these
        /// </summary>
        [JsonProperty("tableNames")]
        public List<String> TableNames
        {
            get
            {
                return MetricRetentionSurface?.MetricsRetentionSurfaceItemEntities?.Select(s => s.TableName).Distinct().ToList();
            }
        }

        /// <summary>
        /// you can supply a func if your environment (Azure Functions)
        /// has a fancy way to get utcnow
        /// 
        /// your func must do the equivalent of 
        /// DateTime.Now.ToString("yyyy")
        /// </summary>
        /// <param name="dateTimeProvider"></param>
        /// <returns></returns>
        public String GetCurrentYear(Func<String> dateTimeProvider = null)
        {
            return dateTimeProvider == null ? CurrentYear : dateTimeProvider();
        }

        /// <summary>
        /// you can supply a func if your environment (Azure Functions)
        /// has a fancy way to get utcnow
        /// 
        /// your func must do the equivalent of 
        /// Convert.ToInt32(DateTime.Now.ToString("MM"));
        /// </summary>
        /// <param name="dateTimeProvider"></param>
        /// <returns></returns>
        public int GetCurrentMonth(Func<int> dateTimeProvider = null)
        {
            return dateTimeProvider == null ? CurrentMonth : dateTimeProvider();
        }

        private String CurrentYear
        {
            get
            {
                return DateTime.Now.ToString("yyyy");
            }
        }

        private int CurrentMonth
        {
            get
            {
                return Convert.ToInt32(DateTime.Now.ToString("MM"));
            }
        }
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class TableStorageEntityRetentionPolicyEntity
    {
        public TableStorageEntityRetentionPolicyEntity()
        {
            // to protect against accidental deletions
            NumberOfDays = 30;
            //TableNames = new List<string>() { ControlChannelConstants.WADPerformanceCountersTable };

            DiagnosticsRetentionSurface = new DiagnosticsRetentionSurfaceEntity();

            TableNames = new List<string>();
            foreach (var item in DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities)
            {
                TableNames.Add(item.TableName);
            }
            PolicyEnforcementMode = policyEnforcementMode.WhatIf;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("policyEnforcementMode")]
        public policyEnforcementMode PolicyEnforcementMode { get; set; }

        /// <summary>
        /// defaults to 65536 just in case woman
        /// </summary>
        [JsonProperty("numberOfDays")]
        public int NumberOfDays { get; set; }

        
        public DateTime OldestRetainedEntity {get; set;}

        public DateTime MostRecentRetainedEntity {get; set;}


        public DiagnosticsRetentionSurfaceEntity DiagnosticsRetentionSurface { get; set; }

        public String GetTicks(Func<String> tickProvider = null)
        {
            return tickProvider == null ? Ticks : tickProvider();
        }

        /// <summary>
        /// as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
        /// with whom as with me no blame is assumed if things go awry i assure you
        /// </summary>
        private String Ticks
        {
            get
            {
                return "0" + DateTime.UtcNow.Subtract(new TimeSpan(NumberOfDays, 0, 0, 0)).Ticks.ToString();
            }
        }

        /// <summary>
        /// you'd be better off supplying these
        /// </summary>
        [JsonProperty("tableNames")]
        public List<String> TableNames
        {
            get
            {
                var ret = new List<string>();

                try
                {
                    ret = DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.Select(s => s.TableName).ToList();

                }
                catch (Exception e)
                {}

                return ret;
            }

            private set
            {
            }
        }

    }

    /// <summary>
    /// disclaimer - these policies heavily leveraged from 
    /// https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
    /// with whom believe you me no blame lies either if things go awry
    /// 
    /// that's why github has discussion forums and issue lists
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class TableStorageRetentionPolicyEntity
    {
        public TableStorageRetentionPolicyEntity()
        {
            this.TableStorageEntityRetentionPolicy = new TableStorageEntityRetentionPolicyEntity();
            this.TableStorageTableRetentionPolicy = new TableStorageTableRetentionPolicyEntity();
            Id = Guid.NewGuid();
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("tableStorageEntityRetentionPolicy")]
        public TableStorageEntityRetentionPolicyEntity TableStorageEntityRetentionPolicy { get; set; }

        [JsonProperty("tableStorageTableRetentionPolicy")]
        public TableStorageTableRetentionPolicyEntity TableStorageTableRetentionPolicy { get; set; }
    }
}
