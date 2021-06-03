using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.entities
{
    public enum WorkflowOperation
    {

        ProvisionAppliance = 0,
        // get the last workflow checkpoint
        GetCurrentWorkflowCheckpoint = 1,
        // inventory the tables
        BeginWorkflow = 2,
        // prepare the data discovery query
        BuildEnvironmentRetentionPolicy = 3,
        // inventory the data
        ApplyEnvironmentRetentionPolicy = 4,
        // delete the data
        CommitRetentionPolicyConfiguration = 5,
        GetV2StorageAccounts = 6,
        CancelWorkflow = 7
    }

    /// <summary>
    /// a command to make available to 
    /// the dashboard with a message for display
    /// in a menu
    /// </summary>
    [JsonObject("MemberSerialization.OptOut")]

    public class AvailableCommandEntity
    {
        public AvailableCommandEntity() { }

        public string AvailableCommandId { get; set; }

        /// <summary>
        /// we only send commands to the correct oid
        /// </summary>
        public string UserOid { get; set; }

        /// <summary>
        /// we only send commands to oid running in the correct tenant
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// we only send commands to the right oid running in the right subscription
        /// </summary>

        public string SubscriptionId { get; set; }
        public string subscriptionName { get; set; }

        public WorkflowOperation WorkflowOperation { get; set; }

        /// <summary>
        /// each command executor must interpret
        /// the supplied json its own way
        /// </summary>
        public string CommandParameterJson { get; set; }

        public string WorklowOperationDisplayMessage { get; set; }
        public string MenuLabel { get; set; }
    }


    /// <summary>
    /// the appliance is commanded via this entity
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class WorkflowOperationCommandEntity
    {
        public WorkflowOperationCommandEntity() { }
        /// <summary>
        /// clients must post the commands with properties matching those
        /// that exist in the workflow state
        /// </summary>
        public AvailableCommandEntity CandidateCommand { get; set; }
        public DateTime TimeStamp { get; set; }

        public string DisplayMessage { get { return CommandCode.ToString("G"); } }
        public WorkflowOperation CommandCode { get; set; }
    }
}
