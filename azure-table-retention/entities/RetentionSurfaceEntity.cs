using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.entities
{
    /// <summary>
    /// aggregates windows azure metrics table descriptors
    /// managed by retention policies
    /// </summary>
    public class MetricRetentionSurfaceEntity
    {
        public MetricRetentionSurfaceEntity()
        {
            Id = Guid.NewGuid();
            MetricsRetentionSurfaceItemEntities = new List<MetricsRetentionSurfaceItemEntity>();
            AggregationPrefixes = new string[]{ "PT1H", "PT1M" };
        }

        public Guid Id { get; set; }
        public string[] AggregationPrefixes { get; set; }
        public List<MetricsRetentionSurfaceItemEntity> MetricsRetentionSurfaceItemEntities  { get; set; }
    }

    /// <summary>
    /// aggregates windows azure diagnostics table descriptors
    /// managed by retention policies
    /// </summary>
    public class DiagnosticsRetentionSurfaceEntity
    {
        public DiagnosticsRetentionSurfaceEntity()
        {
            DiagnosticsRetentionSurfaceEntities = new List<DiagnosticsRetentionSurfaceItemEntity>();

            // generate list of diagnostics tables we will manage with retention policies
            //this.InitializeDiagnosticsRetentionSurface();
             
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public List<DiagnosticsRetentionSurfaceItemEntity> DiagnosticsRetentionSurfaceEntities { get; set; }

        /// <summary>
        /// scaffold reference data
        /// some properties populated by workflow actions 
        /// subsequent to Initialize()
        /// </summary>
        public void InitializeDiagnosticsRetentionSurface()
        {
            DiagnosticsRetentionSurfaceEntities = new List<DiagnosticsRetentionSurfaceItemEntity>()
            {
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "LinuxsyslogVer2v0",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.LinuxsyslogVer2v0,
                    ItemDescription = "Syslog, Linux Virtual Machines",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WADWindowsEventLogsTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WADWindowsEventLogsTable,
                    ItemDescription = "Windows Event Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WADETWEventTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WADETWEventTable,
                    ItemDescription = "Windows ETW Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WadLogsTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WadLogsTable,
                    ItemDescription = "Logs written in code using the trace listener.",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WADDiagnosticInfrastructureLogsTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WADDiagnosticInfrastructureLogsTable,
                    ItemDescription = "Diagnostic monitor and configuration changes.",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WADDiagnosticInfrastructureLogsTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WADDiagnosticInfrastructureLogsTable,
                    ItemDescription = "Diagnostic monitor and configuration changes.",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data")
                },
                new DiagnosticsRetentionSurfaceItemEntity()
                {
                    Id = Guid.NewGuid(),
                    TableName = "WADPerformanceCountersTable",
                    ItemDescriptor = DiagnosticsRetentionSurfaceItemDescriptor.WADPerformanceCountersTable,
                    ItemDescription = "Performance counters.",
                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data")
                }
            };
        }

    }
    /// <summary>
    /// differentiates Table Storage Tables within our model
    /// note - as distinct from azure table storage entities
    /// </summary>
    public enum DiagnosticsRetentionSurfaceItemDescriptor
    {
        // as per https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs
        LinuxsyslogVer2v0,

        LinuxCpuVer2v0,
        LinuxDiskVer2v0,
        LinuxMemoryVer2v0,

        // as per https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data
        WadLogsTable,
        WADPerformanceCountersTable,
        WADWindowsEventLogsTable,
        WADETWEventTable,
        WADDiagnosticInfrastructureLogsTable
    }

    public class RetentionSurfaceItemBase
    {
        public RetentionSurfaceItemBase()
        {
            Id = Guid.NewGuid();
            ItemExists = false;
            ItemWillBeDeleted = false;
        }

        public virtual RetentionSurfaceItemDescriptor ItemType { get; set; }
       
        public string ItemDescription { get; set; }
        public bool ItemExists { get; set; }
        public bool ItemWillBeDeleted { get; set; }
        public Guid Id { get; set; }
        public string StorageAccountId { get; set; }
        public string SuscriptionId { get; set; }
        public string TableName { get; set; }
        public Uri DocumentationLink { get; set; }
    }

    #region the union of the retention surface is on the climb

    public enum RetentionSurfaceItemDescriptor
    {
        IsDiagnosticsTableItem,
        IsMetricsTableItem
    }
    /// <summary>
    /// describes retention policy surface
    /// - table storage tables
    /// - table storage entities
    /// </summary>
    public class DiagnosticsRetentionSurfaceItemEntity : RetentionSurfaceItemBase
    {
        public DiagnosticsRetentionSurfaceItemEntity(): base() { }


        public DiagnosticsRetentionSurfaceItemDescriptor ItemDescriptor { get; set; }

        public override RetentionSurfaceItemDescriptor ItemType { get { return RetentionSurfaceItemDescriptor.IsDiagnosticsTableItem; } }
    } 

    /// <summary>
    /// as per https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage%20WADMetrics%20PT1H%20P10D%20V2S%202020%2001%2005
    /// </summary>
    public class MetricsRetentionSurfaceItemEntity : RetentionSurfaceItemBase
    {

        public MetricsRetentionSurfaceItemEntity(): base() { }

        public int PolicyAgeTriggerInMonths { get; set; }
        public override RetentionSurfaceItemDescriptor ItemType { get { return RetentionSurfaceItemDescriptor.IsMetricsTableItem; } }

        public bool IsDeleted { get; set; }
    }

    #endregion the union of the retention surface is on the climb

}
