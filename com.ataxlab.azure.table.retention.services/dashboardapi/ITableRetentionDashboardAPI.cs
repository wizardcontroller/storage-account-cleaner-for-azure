using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.dashboard;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.services.dashboardapi
{
    public interface ITableRetentionDashboardAPI
    {
        public HttpClient HttpClient { get; set; }

        public Task<OperatorPageModel> GetOperatorPageModel();

        public Task<string> EnsureEasyAuth();

        public Task<T> QueryAppliance<T>(HttpMethod method, string endpoint, string jsonPayload = "") where T : new();

        /// <summary>
        /// query the appliance for its status
        /// </summary>
        /// <returns></returns>

        public Task<WorkflowCheckpointDTO> GetCurrentWorkflowCheckpoint();

        public EnvironmentDiscoveryResult GetEnvironmentDiscoveryResult(string discoveryInstanceId = ControlChannelConstants.DefaultApplianceWorkflowInstanceId);
        public Task<string> GetCurrentEasyAuthToken();
        public EnvironmentDiscoveryResult StartEnvironmentDiscovery(string discoveryInstanceId = ControlChannelConstants.DefaultApplianceWorkflowInstanceId, string startDiscoveryEndpoint = ControlChannelConstants.WorkflowEntryPoint);

        public Task<List<AvailableCommand>> ProvisionAppliance(AvailableCommand command);
        Task<List<AvailableCommand>> BeginEnvironmentDiscovery(AvailableCommand candidateCommand);
        Task<List<AvailableCommand>> BeginWorkflow(AvailableCommand candidateCommand);
        Task<List<StorageAccountDTO>> GetStorageAccounts(string subscriptionid, string impersonationToken, string requestingOid);
        Task<ApplianceSessionContext> GetApplianceContext(string tenantId, string oid);
        Task<ApplianceSessionContext> SetApplianceSessionContext(string tenantid, string oid, ApplianceSessionContext ctx);
        string GetTenantIdFromUserClaims();
        string GetUserOidFromUserClaims();
        Task<string> GetTemplateUrlForRoute(string endoint);
        Task<List<AvailableCommand>> EnsureWorkflowOperation(AvailableCommand command);
    }
}
