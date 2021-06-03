using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using DurableTask.Core;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models;

namespace com.ataxlab.functions.table.retention.dashboard.Models
{
    public class ScaffoldingContext : DbContext
    {
        public ScaffoldingContext(DbContextOptions<ScaffoldingContext> options) : base(options)
        {

        }

        public DbSet<SubscriptionDTO> Subscriptions { get; set; }
        public DbSet<WorkflowCheckpointStatus> CheckpointStatus { get; set; }

        public DbSet<WorkflowCheckpointDTO> Checkpoint { get; set; }

        public DbSet<AvailableCommand> AvailableCommands { get; set; }

        public DbSet<DurableOrchestrationStatusDTO> CurrentOrchestrationStatus { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.DurableOrchestrationStateDTO> DurableOrchestrationStateDTO { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.auth.ApplianceSessionContext> ApplianceSessionContext { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.azuremanagement.ScaffoldingStorageAccount> StorageAccountDTO { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.azuremanagement.StorageAccountDTO> StorageAccountDTO_1 { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.TableStorageEntityRetentionPolicy> TableStorageEntityRetentionPolicy { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.TableStorageEntityRetentionPolicyEnforcementResult> TableStorageEntityRetentionPolicyEnforcementResult { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.TableStorageTableRetentionPolicyEnforcementResult> TableStoragTableRetentionPolicyEnforcementResult { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.TableStorageTableRetentionPolicy> TableStorageTableRetentionPolicy { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.auth.RetentionPolicyTupleContainer> RetentionPolicyTupleContainer { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.auth.ApplianceJobOutput> ApplianceJobOutput { get; set; }

        public DbSet<TableStorageRetentionPolicy> TableStorageRetentionPolicy { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.MetricsRetentionSurfaceItemEntity> MetricsRetentionSurfaceItemEntity { get; set; }

        public DbSet<com.ataxlab.azure.table.retention.models.models.DiagnosticsRetentionSurfaceItemEntity> DiagnosticsRetentionSurfaceItemEntity { get; set; }

    }
}
