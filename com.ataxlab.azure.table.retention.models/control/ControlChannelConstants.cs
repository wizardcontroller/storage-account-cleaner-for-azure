using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.control
{

    public enum WorkflowCheckpointIdentifier
    {
        // the appliance has never initialized its checkpoints 
        UnProvisioned,
        // paused waiting for start event
        CanStartEnvironmentDiscovery,
        // running
        EnvironmentDiscoveryStarted,
        // completed
        EnvironmentDiscoveryCompleted,
        // paused waiting for start event
        CanGenerateEnvironmentRetentionPolicy,
        // running
        GenerateEnvironmentRetentionPolicyStarted,
        // completed
        GenerateEnvironmentRetentionPolicyCompleted,
        // paused waiting for start event
        CanApplyEnvironmentRetentionPolicy,
        // running
        ApplyEnvironmentRetentionPolicyStarted,
        // completed
        ApplyEnvironmentRetentionPolicyCompleted,
        // paused waiting for start event
        CanCommitRetentionPolicyConfiguration,
        // running
        CommitRetentionPolicyConfigurationStarted,
        // completed
        CommitRetentionPolicyConfigurationCompleted,
        GetV2StorageAccountsStarted
    }

    public class ControlChannelRouteParameter
    {
        public string EndPoint { get; set; }
        public string RouteFormatTemplate { get; set; }
    }

    public class ControlChannelConstants
    {
        #region durable entities
        
        public const string DefaultWorkflowCheckpointEntityKey = "DefaultWorkflowCheckpointEntityKey";
        public const string DefaultWorkflowCheckpointEntityName = "DefaultWorkflowCheckpointEntityName";

        public const string DiscoveryStatus_Finished = "DiscoveryFinished";
        #endregion durable entities


        #region endpoints

        /// <summary>
        /// activity that returns appliance context entity state
        /// </summary>
        public const string GetCurrentApplianceContextActivityEndpoint = "GetCurrentApplianceContextActivityEndpoint";

        public const string SetEntityRetentionPolicyForStorageAccountEndpoint = "SetEntityRetentionPolicyForStorageAccount";
        public const string SetEntityRetentionPolicyForStorageAccountRouteFormatTemplate = "/{0}/{1}/{2}/{3}";
        public const string SetEntityRetentionPolicyForStorageAccountRouteTemplate = "/{tenantId}/{oid}/{tableStorageEntityRetentionPolicyEntityId}/{diagnosticsRetentionSurfaceEntityId}";

        public const string SetTableRetentionPolicyForStorageAccountEndpoint = "SetTableRetentionPolicyForStorageAccount";
        public const string SetTableRetentionPolicyForStorageAccountRouteFormatTemplate = "/{0}/{1}/{2}/{3}";
        public const string SetTableRetentionPolicyForStorageAccountRouteTemplate = "/{tenantId}/{oid}/{tableStorageEntityRetentionPolicyEntityId}/{diagnosticsRetentionSurfaceEntityId}";

        #region obsolete
        public const string RetentionPolicyPostEndpoint = "PostRetentionPolicyEndpoint";
        public const string RetentionPolicyEndpoint = "RetentionPolicyEndpoint";
        public const string RetentionPolicyRouteFormatTemplate = "/{0}/{1}/{2}/{3}";
        public const string RetentionPolicyRouteTemplate = "/{tenantId}/{subscriptionId}/{storageAccountId}/{oid}";
        #endregion obsolete

        public const string ApplianceContextEndpoint = "ApplianceContextEndpoint";
        public const string ApplianceContextRouteFormatTemplate = "/{0}/{1}";
        public const string ApplianceContextRouteTemplate = "/{tenantId}/{oid}";

        public const string GetApplianceLogEntriesRouteTemplate = "/{tenantId}/{oid}/{offset}/{pageSize}/{pageCount}";

        public const string QueryOrchestrationStatusEndpoint = "QueryWorkflowStatus";
        public const string QueryOrchestrationStatusRouteFormatTemplate = "/{0}/{1}/{2}";
        public const string QueryOrchestrationStatusRouteTemplate = "/{tenantId}/{oid}/{fromDays}";

        public const string ResetWorkflowsEndpoint = "PurgeWorkflows";

        public const string QueryWorkflowCheckpointStatusEndpoint = "QueryWorkflowCheckpointStatus";
        public const string QueryWorkflowCheckpointStatusRouteFormatTemplate = "/{0}/{1}";
        public const string QueryWorkflowCheckpointStatusRouteTemplate = "/{tenantId}/{oid}";

        public const string QueryWorkflowEditModeCheckpointStatusEndpoint = "QueryWorkflowEditModeCheckpointStatus";
        public const string QueryWorkflowEditModeCheckpointStatusRouteFormatTemplate = "/{0}/{1}";
        public const string QueryWorkflowEditModeCheckpointStatusRouteTemplate = "/{tenantId}/{oid}";

        public const string ApplyTableEntityRetentionPolicyTuplesEndpoint = "ApplyTableEntityRetentionPolicyTuples";
        public const string ApplyTableRetentionPolicyTuplesEndpoint = "ApplyTableRetentionPolicyTuples";

        public const string BuildRetentionPolicyTuplesEndpoint = "BuildRetentionPolicyTuples";
        public const string GetAllV2StorageAccountsEndpoint = "GetAllV2StorageAccounts";
        public const string GetV2StorageAccounts = "GetV2StorageAccounts";

        // name of the function to call as the starter 
        public const string ApplicationControlChannelEndpoint = "WorkflowOperator";
        public const string ApplicationControlChannelFormatTemplate = "/{0}/{1}";
        public const string ApplicationControlChannelRouteTemplate = "/{tenantId}/{oid}";

        public const string DeleteWorkflowCheckpointEditModeEndPoint = "DeleteWorkflowCheckpointEditMode";
        public const string DeleteWorkflowCheckpointEditModeRouteFormatTemplate = "/{0}/{1}";
        public const string DeleteWorkflowCheckpointEditModeRouteTemplate = "/{tenantId}/{oid}";

        // name of activity function to call to command appliance to 
        // enter environment discovery state
        //public const string WorkflowEntryPoint = "DiscoverEnvironmentWithDefaultWADiagnosticsPolicy";
        public const string WorkflowEntryPoint = "WorkflowEntryPoint";
        public const string WorkflowEntrypointDebug = "DebugWorkflowEntryPoint";
        public const string AbandonOrchestrationsEndpoint = "AbandonOrchestrations";
        #endregion endpoints

        #region authorization

        public const string CLAIM_NAMEIDENTIFIER = "nameidentifier";
        public const string CLAIM_OID = "objectidentifier";
        public const string CLAIM_TENANT_UTID = "utid";
        public const string SESSION_ACCESS_TOKEN = "USERTOKEN";
        public const string SESSION_IDTOKEN = "IDTOKEN";
        public const string SESSION_IMPERSONATION_TOKEN = "IMPERSONATIONTOKEN";
        public const string SESSION_KEY_EASYAUTHTOKEN = "EasyAuthToken";
        public const string SESSION_SELECTED_SUBSCRIPTION = "SELECTED_SUBSCRIPTION";
        
        //public const string COOKIE_CURRENTSUBSCRIPTION = "SelectedSubscription";

        public const string HEADER_CURRENTSUBSCRIPTION = "x-table-retention-current-subscription";
        public const string HEADER_CURRENT_STORAGE_ACCOUNT = "x-table-retention-current-storage-account";
        public const string HEADER_IMPERSONATION_TOKEN = "x-table-retention-mgmt-impersonation";
        public const string HEADER_AUTHORIZATION = "authorization";
        public const string HEADER_X_ZUMO_AUTH = "X-ZUMO-AUTH";
        public const string HEADER_X_MS_TOKEN_AAD_ACCESS_TOKEN = "x-ms-token-aad-access-token";

        #endregion authorization
        /// <summary>
        ///  as per singleton azure durable functions 
        ///  https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons?tabs=csharp
        ///  we only want one instance of discovery codepath running at a time
        ///  so we use the same instance id when sendng environment discovery commands
        ///  to the appliance
        /// </summary>
        public const string DefaultApplianceWorkflowInstanceId = "DefaultWorkflowId";

        #region workflow events
        public const string WorkflowEvent_ApplyRetentionPolicyTuples = "WorkflowEvent_ApplyRetentionPolicyTuples";
        public const string WorkflowEvent_BuildRetentionPolicyTuples = "WorkflowEvent_BuildRetentionPolicyTuples";
        public const string WorkflowEvent_GetV2StorageAccounts = "WorkflowEvent_GetV2StorageAccounts";
        public const string WorkflowEvent_CommitRetentionPolicy = "WorkflowEvent_CommitRetentionPolicy";
        public const string CANCEL_WORKFLOW = "WorkflowEvent_Cancel_Workflow";
        #endregion workflow events

        // don't do this because it the IDurableClientFactory.CreateClient will throw nulls unless this value is constant
        // this forces a certain name on the task hub
        // public static string TASKHUBNAME = System.Environment.GetEnvironmentVariable( "tableretentiontaskhubname");
        public const string TASKHUBNAME = "tableretentionhub";
        public const string STORAGEIMPERSONATION = "https://storage.azure.com/user_impersonation";
        private const string MANAGEMENTAPIIMPERSATION = "https://login.microsoftonline.com/common/oauth2/authorize/user_impersonation";
        public const string AZUREMANAGEMENT_USERIMPERSONATION = "https://management.azure.com/user_impersonation"; // "https://storage.azure.com/user_impersonation"; // "https://management.azure.com/user_impersonation";
        public const string AZUREMANAGEMENT_BASEURL_NO_TRAILING_SLASH = "https://management.azure.com";

        public const string DialogChooseSubscription = "DialogChooseSubscription";

        // windows azure diagnostics stores entities in this table that we may want to age
        public const string WADPerformanceCountersTable = "WADPerformanceCountersTable";
    }
}
