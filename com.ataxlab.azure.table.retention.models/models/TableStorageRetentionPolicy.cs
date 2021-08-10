using com.ataxlab.azure.table.retention.models.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace com.ataxlab.azure.table.retention.models
{
    ///todo enumerate these numerically wherever they appear
    [JsonConverter(typeof(StringEnumConverter))]
    public enum policyEnforcementMode { WhatIf, ApplyPolicy}

    public class TableStorageTableRetentionPolicy
    {
        public TableStorageTableRetentionPolicy()
        {

            DeleteOlderTablesThanCurrentMonthMinusThis = 2;
            PolicyEnforcementMode = policyEnforcementMode.WhatIf;
            Id = Guid.NewGuid();

            MetricRetentionSurface = new MetricRetentionSurfaceEntity();
            TableNames = new List<string>();
        }


        [JsonIgnore]
        public Guid Id { get; set; }

        public MetricRetentionSurfaceEntity MetricRetentionSurface { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public policyEnforcementMode PolicyEnforcementMode { get; set; }

        /// <summary>
        /// defaults to 65536 just in case man
        /// </summary>
        public int DeleteOlderTablesThanCurrentMonthMinusThis { get; set; }

        public DateTime OldestRetainedTable {get; set;}

        public DateTime MostRecentRetainedTable {get; set;}

        /// <summary>
        /// you'd be better off supplying these
        /// </summary>
        [NotMapped]
        public List<String> TableNames {
            get
            {
                return MetricRetentionSurface.MetricsRetentionSurfaceItemEntities.Select(s => s.TableName).ToList();
            }
            set
            {
                // we won't permit this
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

    public class TableStorageEntityRetentionPolicy
    { 
        public TableStorageEntityRetentionPolicy()
        {
            // to protect against accidental deletions
            NumberOfDays = 30;
            DiagnosticsRetentionSurface = new DiagnosticsRetentionSurfaceEntity();

            TableNames = new List<string>();
            foreach (var item in DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities)
            {
                TableNames.Add(item.TableName);
            }
            PolicyEnforcementMode = policyEnforcementMode.WhatIf;
            Id = Guid.NewGuid();
        }


        [JsonIgnore]
        [Key]
        
        public Guid Id { get; set; }

        public DiagnosticsRetentionSurfaceEntity DiagnosticsRetentionSurface { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public policyEnforcementMode PolicyEnforcementMode { get; set; }

        /// <summary>
        /// defaults to 65536 just in case woman
        /// </summary>
        public int NumberOfDays { get; set; }

        public DateTime OldestRetainedEntity {get; set;}

        public DateTime MostRecentRetainedEntity {get; set;}

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
                return "0" + DateTime.UtcNow.Subtract(new TimeSpan(NumberOfDays,0,0,0)).Ticks.ToString();
            }
        }

        /// <summary>
        /// you'd be better off supplying these
        /// </summary>
        [NotMapped]
        public List<String> TableNames { get; set; }

    }

    /// <summary>
    /// disclaimer - these policies heavily leveraged from 
    /// https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
    /// with whom believe you me no blame lies either if things go awry
    /// 
    /// that's why github has discussion forums and issue lists
    /// </summary>
    public class TableStorageRetentionPolicy
    {
        public TableStorageRetentionPolicy()
        {
            this.TableStorageEntityRetentionPolicy = new TableStorageEntityRetentionPolicy();
            this.TableStorageTableRetentionPolicy = new TableStorageTableRetentionPolicy();
            Id = Guid.NewGuid();
        }

        [JsonIgnore]
        public Guid Id { get; set; }

        public TableStorageEntityRetentionPolicy TableStorageEntityRetentionPolicy { get; set; }

        public TableStorageTableRetentionPolicy TableStorageTableRetentionPolicy { get; set; }
    }
}
