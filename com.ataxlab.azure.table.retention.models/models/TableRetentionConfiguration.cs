using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models
{
    public interface ITableRetentionConfiguration
    {
        /// <summary>
        /// a list of Azure Table Storage Table Name Fragments
        /// without any wildcard characters
        /// 
        /// the operator supplies this list
        /// and it is diff compared against
        /// the list of tables in a storage account
        /// </summary>
        List<String> WADDIagnosticsTableNameMatchPatterns { get; set; }
    }

    /// <summary>
    /// supply an arbitrary list of table names
    /// to apply retention policy for
    /// </summary>
    public class TableRetentionConfiguration : ITableRetentionConfiguration
    {
        public TableRetentionConfiguration()
        { }


        /// <summary>
        /// a list of Azure Table Storage Table Name Fragments
        /// without any wildcard characters
        /// 
        /// the operator supplies this list
        /// and it is diff compared against
        /// the list of tables in a storage account
        /// </summary>
        public List<String> WADDIagnosticsTableNameMatchPatterns { get; set; }
    }

    /// <summary>
    /// supply a default list of table names
    /// used by Windows Azure Diagnostics
    /// </summary>
    public class DefaultTableRetentionConfiguration : TableRetentionConfiguration
    {
        public String WADMetrics = "WADMetrics";
        public String WADDiagnosticInfrastructureLogsTable = "WADDiagnosticInfrastructureLogsTable";
        public String WADWindowsEventLogsTable = "WADWindowsEventLogsTable";
        public String WADPerformanceCountersTable = "WADPerformanceCountersTable";
        public List<String> WADMetricsTableNameMatchPatterns { get; set; }
        private List<String> defaultDiagnosticsList;
        private List<String> defaultMetricsist;

        /// <summary>
        /// initialize with the default table names used by 
        /// Windows Azure Diagnostics
        /// </summary>
        public DefaultTableRetentionConfiguration() : base()
        {
            InitializeDefaultLists();
        }

        private void InitializeDefaultLists()
        {
            defaultMetricsist = new List<string>()
            {
                WADMetrics
            };
            WADMetricsTableNameMatchPatterns = defaultMetricsist;

            defaultDiagnosticsList = new List<string>()
            {
                WADDiagnosticInfrastructureLogsTable,WADWindowsEventLogsTable,WADPerformanceCountersTable
            };

            WADDIagnosticsTableNameMatchPatterns = defaultDiagnosticsList;
        }
    }
}
