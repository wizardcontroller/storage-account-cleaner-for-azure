using com.ataxlab.azure.table.retention.models.control;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using com.ataxlab.azure.table.retention.state.entities;
using System.Linq;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using com.ataxlab.azure.table.retention.services.authorization;

using System.Security.Claims;
using com.ataxlab.azure.table.retention.models.extensions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using com.ataxlab.functions.table.retention.utility;
using Microsoft.Azure.Management.Storage;
using System.Net.Http.Headers;
using com.ataxlab.functions.table.retention.entities;
using WorkflowOperation = com.ataxlab.functions.table.retention.entities.WorkflowOperation;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace com.ataxlab.functions.table.retention.services
{
    public interface ITableRetentionApplianceEngine
    {
        ITableRetentionApplianceActivities ActivitiesEngine { get; }

        public Task<WorkflowCheckpoint> ProvisionDevice(IDurableEntityClient durableEntityClient,
                                                        string tenantid, string subscriptionid,
                                                        string userOid, ApplianceSessionContextEntityBase ctx);
        public Task<HttpResponseMessage> GetResponseForBeginWorkflow(IDurableEntityClient entityClient, IDurableClient durableClient, IDurableOrchestrationClient starter, string tenantId, string oid);

        public Task<bool> ApplyAuthorizationStrategy(HttpRequestHeaders req, ClaimsPrincipal claimsPrincipal, SubscriptionDTO subscription = null);
        public string GetAuthorizationHeader(HttpRequestHeaders req);
        public Task<bool> ValidateClaimsPrincipalFromHeaders(HttpRequestMessage req, SubscriptionDTO subscription = null);
        public Task<Tuple<bool, ClaimsPrincipal>> IsAuthorized(string authorizationHeader, SubscriptionDTO subscription = null);
        // public Task<EntityStateResponse<WorkflowCheckpointEditMode>> GetCurrentEditModeWorkflowCheckpoint(IDurableEntityClient durableClient, SubscriptionDTO subscription = null);
        public Task<bool> ValidateTransition(IDurableEntityClient durableClient, WorkflowOperationCommandEntity command, string tenantId,
                                                string subscriptionId, string oid);
        public bool IsValidTransition(WorkflowOperationCommandEntity candidateCommand, List<AvailableCommandEntity> availbleCommands, SubscriptionDTO subscription = null);

        public Task<List<AvailableCommandEntity>> GetAvailableCommandsForTransition(WorkflowOperation candidateOperation, string subscriptionId,
                                                                                string tenantid, string oid);

        public Task<List<AvailableCommandEntity>> GetAvailableCommandsForTransition(WorkflowOperation candidateOperation, SubscriptionDTO subscription = null);

        public Task<OrchestrationStatusQueryResult> QueryInstancesAsync(IDurableOrchestrationClient client, OrchestrationStatusQueryCondition queryFilter);
        public JObject GetCustomEventForJObject(string policyStatus);
        Task<bool> CanGetStorageAccountsForUser(string impersonate, ApplianceSessionContextEntityBase ctx);
        Task<TableRetentionApplianceEngine.ApplianceInitializationResult> ValidateApplianceContextForUser(string tenantId, string oid, List<Claim> userClaims, string impersonationToken, ApplianceSessionContextEntityBase ctx);
        Task<string> GetImpersonationTokenFromHeaders(HttpRequestHeaders headers);
        Task<string> GetUserOidFromClaims(IEnumerable<Claim> claims);

        Task<TableStorageRetentionPolicyEntity> GetRetentionPolicy(string tenantId, string subscriptionId, string storageAccountId, string oid, IDurableClient durableClient);
        Task<TableStorageRetentionPolicyEntity> SetGetRetentionPolicy(string tenantId, string subscriptionId, string storageAccountId, string oid, TableStorageRetentionPolicyEntity policy, IDurableClient durableClient);

        #region durable entity operations



        Task<EntityId> GetEntityIdForUser<T>(string tenantId, string oid) where T : new();
        Task<EntityStateResponse<ApplianceSessionContextEntity>> GetApplianceContextForUser(string tenantId, string oid, IDurableEntityClient durableClient);
        Task<EntityStateResponse<WorkflowCheckpoint>> GetWorkflowCheckpointEntityForUser(IDurableEntityClient durableClient, EntityId entityId, string subscriptionid, string tenantid, string oid);
        Task<EntityStateResponse<WorkflowCheckpointEditMode>> GetWorkflowCheckpointEditModeEntityForUser(IDurableEntityClient durableClient, EntityId entityId, string subscriptionid, string tenantid, string oid);
        Task<HttpResponseMessage> GetWorkflowEditModeCheckpointResponseForUser(IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid);
        Task<HttpResponseMessage> GetWorkflowCheckpointResponseForUser(IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid);
        Task<bool> SetWorkflowState(IDurableClient durableEntityClient, List<AvailableCommandEntity> commands, string message, WorkflowCheckpointIdentifier commandCode, SubscriptionDTO subscription = null);
        Task SetWorkflowCheckpointForUser(IDurableEntityClient durableEntityClient, EntityId entityId, string tenantId, string userOid, string subscriptionId, WorkflowOperation workflowOperation);

        Task<ApplianceSessionContextEntityBase> SetApplianceContextForUser(string tenantId, string oid, ApplianceSessionContextEntityBase ctx, IDurableClient durableEntityClient);
        Task<HttpResponseMessage> GetApplianceSessionContextResponseForuser(string tenantId, string oid, IDurableEntityClient durableClient);
        Task<HttpResponseMessage> GetResposeForPostedApplianceSessionContext(string impersonationToken, string tenantId, string oid, List<Claim> claims, IDurableClient durableClient, string commandJson);
        Task<HttpResponseMessage> GetResponseForWorkflowOperator(IDurableOrchestrationClient starter, IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid, string commandJson, string impersonationToken);
        Task<EntityStateResponse<WorkflowCheckpoint>> GetStateForUpdateWorkflowCheckpoints(IDurableEntityClient durableEntityClient, string tenantid, string subscriptionid, string userOid, WorkflowOperation operation);
        Task Log(JobOutputLogEntry logEntry, string tenantId, string oid, IDurableEntityClient entityClient);
        Task<bool> SetCurrentJobOutput(string tenantId, string oid, ApplianceSessionContextEntity ctx, IDurableClient durableClient);
        Task<string> GetHttpContextHeaderValueForKey(string headerKey);
        #endregion durable entity operations
    }

    /// <summary>
    /// place to put code that would otherwise be private
    /// and clutter up public interaces
    /// </summary>
    public class TableRetentionApplianceEngine : ITableRetentionApplianceEngine
    {
        ITableRetentionApplianceActivities tableRetentionApplianceActivities;
        private IHttpContextAccessor CurrentHttpContext { get; set; }
        ILogger<TableRetentionApplianceEngine> log;


        // as per https://damienbod.com/2020/09/24/securing-azure-functions-using-azure-ad-jwt-bearer-token-authentication-for-user-access-tokens/
        AzureADJwtBearerValidation AzureADJwtBearerValidationService { get; set; }
        public ITableRetentionApplianceActivities ActivitiesEngine { get; set; }

        //IDurableClient durableEntityClient;
        //IConfiguration _configuration;
        #region test generation support
        /// <summary>
        /// this exists as a way to support easy
        /// eastantiation during unit tests
        /// </summary>
        protected internal TableRetentionApplianceEngine(IHttpContextAccessor ctx = null)
        {
            this.CurrentHttpContext = ctx;
        }

        public static ITableRetentionApplianceEngine GetUnitTestableEngine()
        {
            return new TableRetentionApplianceEngine();
        }

        #endregion test generation support

        public async Task<string> GetHttpContextHeaderValueForKey(string headerKey)
        {
            var ret = string.Empty;

            var ctx = this.CurrentHttpContext.HttpContext;
            var header = ctx.Request.Headers.Where(w => w.Key.ToLowerInvariant().Equals(headerKey.ToLowerInvariant()))
                            .FirstOrDefault()
                            .Value;
            ret = header;
            return await Task.FromResult<string>(ret);
        }

        public async Task<string> GetUserOidFromClaims(IEnumerable<Claim> claims)
        {
            var oid = String.Empty;
            try
            {

                var result = claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_OID)).FirstOrDefault()?.Value;
                oid = result;

            }
            catch (Exception e)
            {
                return await Task.FromResult(String.Empty);
            }

            return await Task.FromResult(oid);
        }

        /// <summary>
        /// expect the client to post the impersonation token in headers
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<string> GetImpersonationTokenFromHeaders(HttpRequestHeaders headers)
        {
            var impersonationToken = String.Empty;
            try
            {

                IEnumerable<string> listValues;
                var r = headers.TryGetValues(ControlChannelConstants.HEADER_IMPERSONATION_TOKEN, out listValues);

                var keysList = new List<string>();
                foreach (var h in headers)
                {
                    keysList.Add(h.Key);
                }

                var impersonate = headers.Where(w => w.Key.ToLower().Equals(ControlChannelConstants.HEADER_IMPERSONATION_TOKEN.ToLower())).Select(s => s.Value)?.FirstOrDefault();
                impersonationToken = impersonate?.FirstOrDefault()?.TrimStart(',')?.Trim();

            }
            catch (Exception e)
            {
                return await Task.FromResult(String.Empty);
            }

            return await Task.FromResult(impersonationToken);
        }


        public async Task<string> GetTenantIdFromUserClaims(List<Claim> claims)
        {
            log.LogInformation("getting tenant id from user claims");
            var tenantId = claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_TENANT_UTID)).FirstOrDefault()?.Value;
            return await Task.FromResult(tenantId);
        }


        public async Task<bool> CanGetStorageAccountsForUser(string impersonate, ApplianceSessionContextEntityBase ctx)
        {
            var storageAccessResult = false;
            try
            {
                log.LogTrace("testing USERTOKEN with subscription {0}", ctx.SelectedSubscriptionId);

                var creds = new Microsoft.Rest.TokenCredentials(impersonate);
                StorageManagementClient storage = new StorageManagementClient(creds);


                storage.SubscriptionId = ctx.SelectedSubscriptionId;

                log.LogInformation("testing USERTOKEN StorageManagementClient for subscription {0}", storage?.SubscriptionId);
                var storageAccounts = await storage?.StorageAccounts?.ListAsync();
                if (storageAccounts == null || storageAccounts.Count() == 0)
                {
                    log.LogWarning("failed to access any storage accounts with the supplied token");
                    return false;
                }

                // validate tenant id
                foreach (var acct in storageAccounts)
                {
                    // storage account ids contain subscription ids
                    if (!acct.Id.Contains(ctx.SelectedSubscriptionId))
                    {
                        log.LogWarning("retrieved storage account does not exist in selected subscription");
                        return false;
                    }
                }



                List<bool> testResults = new List<bool>();
                foreach (var acct in ctx.SelectedStorageAccounts)
                {
                    // adds false to the list if id missing
                    testResults.Add(storageAccounts.Where(s => s.Id.Equals(acct.Id)).Any());
                }

                // result is false if any of the test results were false
                storageAccessResult = !testResults.Where(w => w == false).Any();
                log.LogTrace("storage accounts found {0}", storageAccounts?.Count());

            }
            catch (Exception e)
            {
                /*
                 * 
                 * The access token has been obtained for wrong audience or resource
                 * 'appid'. 
                 * It should exactly match with one of the allowed audiences 
                 * 'https://management.core.windows.net/','https://management.core.windows.net',
                 * 'https://management.azure.com/','https://management.azure.com'.
                 **/
                storageAccessResult = false;
                log.LogError("storage experiment failed due to {0}", e.Message);
            }

            return storageAccessResult;
        }

        public enum ApplianceInitializationResult { SUCCEEDED, FAILED_BAD_TENANT_ID, FAILED_BAD_OID, FAILED_INSUFFICIENT_PERMISSIONS }

        /// <summary>
        /// validate the calling user has passed sufficient data to access the storage accounts 
        /// passed to the appliance
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="oid"></param>
        /// <param name="userClaims"></param>
        /// <param name="impersonationToken"></param>
        /// <returns></returns>
        public async Task<ApplianceInitializationResult> ValidateApplianceContextForUser(string tenantId,
            string oid, List<Claim> userClaims, string impersonationToken,
            ApplianceSessionContextEntityBase ctx)
        {
            var ret = ApplianceInitializationResult.SUCCEEDED;

            // todo perform further validation checks
            try
            {
                var result = await this.CanGetStorageAccountsForUser(impersonationToken, ctx);
                if (result == false)
                {
                    log.LogWarning("cannot get storage accounts for user");
                    return ApplianceInitializationResult.FAILED_INSUFFICIENT_PERMISSIONS;
                }
                else
                {
                    log.LogWarning("got storage accounts for user");
                    return ApplianceInitializationResult.SUCCEEDED;
                }
            }
            catch (Exception e)
            {
                log.LogError("problem validating stoage account permissions {0}");
                return ApplianceInitializationResult.FAILED_INSUFFICIENT_PERMISSIONS;
            }

            //return await Task.FromResult<ApplianceInitializationResult>(ret);
        }

        public async Task<HttpResponseMessage> GetResponseForWorkflowOperator(IDurableOrchestrationClient starter, IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid, string commandJson, string impersonationToken)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                // dequeue the current command
                bool isValidTransition = false;

                log.LogInformation("posted command null {0}", commandJson == null);
                var command = await commandJson.FromJSONStringAsync<WorkflowOperationCommandEntity>();
                var subscriptionId = await this.GetHttpContextHeaderValueForKey(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION);

                var instanceId = CrucialExtensions.HashToGuid(tenantId, oid, subscriptionId);

                log.LogInformation("posted command code {0}", command.CommandCode.ToString("G"));

                var applianceContextEntityId = await this.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);
                var applianceContext = await this.GetApplianceContextForUser(tenantId, oid, durableEntityClient);
                if (applianceContext.EntityExists)
                {
                    log.LogInformation("dispatching command with valid appliance context");

                    try
                    {
                        var orchestrationStatus = await durableClient.GetStatusAsync(instanceId.ToString(), showHistory: true);
                        if (orchestrationStatus == null || orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Failed
                            ||
                            orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                        {
                            // orchestration failed - apply the begin workflow condition
                            httpResponseMessage = await this.GetResponseForBeginWorkflow(durableEntityClient, durableClient, starter, tenantId, oid);
                            //return await this
                            //    .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);

                        }
                    }
                    catch (Exception e)
                    {
                        log.LogWarning("exception handling nonexistent workflow condition {0}", e.Message);
                    }

                    var canGetStorage = await CanGetStorageAccountsForUser(impersonationToken, applianceContext.EntityState);
                    // validate the syntax - the prefetch logic
                    isValidTransition = await this.ValidateTransition(durableClient, command, tenantId,
                    applianceContext.EntityState.SelectedSubscriptionId, oid);
                    log.LogInformation("is valid transition? {0}", isValidTransition);

                    // the instruction dispatcher
                    if (isValidTransition)
                    {
                        subscriptionId = applianceContext.EntityState.SelectedSubscriptionId;



                        switch (command.CommandCode)
                        {
                            case WorkflowOperation.CancelWorkflow:
                                {
                                    // report the workflow status - redundant
                                    log.LogInformation("executing operation {0}", WorkflowOperation.CancelWorkflow.ToString("G"));

                                    try
                                    {
                                        log.LogInformation("workflow cancel operation started.");


                                        try
                                        {
                                            await durableClient.RaiseEventAsync(instanceId.ToString(),
                                                                                ControlChannelConstants.CANCEL_WORKFLOW,
                                                                                command);

                                            var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                    oid, command.CommandCode);
                                            log.LogInformation("cancel signalled");
                                        }
                                        catch (Exception e)
                                        {
                                            log.LogWarning("error signalling workflow {0}", e.Message);
                                        }



                                        try
                                        {
                                            var currentState = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId
                                                , subscriptionId, oid, WorkflowOperation.CancelWorkflow);

                                            var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                        oid, command.CommandCode);
                                            log.LogInformation("checkpoint status reset");
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                        log.LogInformation("workflow cancel operation completed.");
                                    }
                                    catch (Exception e)
                                    {
                                        // maybe there's no running instance
                                        log.LogWarning("issue signalling cancel workflow {0}", e.Message);

                                        // validate the user passed a usable token to run the workflow
                                        return await this
                                            .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                                    }
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);

                                }
                            case WorkflowOperation.GetCurrentWorkflowCheckpoint:
                                {
                                    // report the workflow status - redundant
                                    log.LogInformation("executing operation {0}", WorkflowOperation.GetCurrentWorkflowCheckpoint.ToString("G"));

                                    var entityId = await this.GetEntityIdForUser<WorkflowCheckpoint>(tenantId, oid);

                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);

                                }

                            case WorkflowOperation.BeginWorkflow:
                                {
                                    // begin the orchestration - there can be only one
                                    // TODO - proor to beginning check that 
                                    // this is one of the available commands
                                    log.LogInformation("executing operation {0}", WorkflowOperation.BeginWorkflow.ToString("G"));
                                    // validate the user passed a usable token to run the workflow
                                    httpResponseMessage = await this.GetResponseForBeginWorkflow(durableEntityClient, durableClient, starter, tenantId, oid);
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);

                                }
                            case WorkflowOperation.GetV2StorageAccounts:
                                {
                                    // begin the GetV2StorageAccounts - there can be only one
                                    // TODO - proor to beginning check that 
                                    // this is one of the available commands
                                    log.LogInformation("executing operation {0}", WorkflowOperation.GetV2StorageAccounts.ToString("G"));

                                    var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                oid, command.CommandCode);
                                    var config = new ActivityConfig()
                                    {
                                        ActivityContext = applianceContext.EntityState,
                                        AuthToken = impersonationToken,
                                        WorkflowOperation = command
                                    };

                                    await durableClient.RaiseEventAsync(instanceId.ToString(),
                                                                        ControlChannelConstants.WorkflowEvent_GetV2StorageAccounts,
                                                                        config);


                                    // validate the user passed a usable token to run the workflow
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                                }
                            case WorkflowOperation.BuildEnvironmentRetentionPolicy:
                                {
                                    // send the appropriate signal to unblock the workflow
                                    log.LogInformation("executing operation {0}", WorkflowOperation.BuildEnvironmentRetentionPolicy.ToString("G"));

                                    var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                oid, command.CommandCode);

                                    var config = new ActivityConfig()
                                    {
                                        ActivityContext = applianceContext.EntityState,
                                        AuthToken = impersonationToken,
                                        WorkflowOperation = command
                                    };
                                    await durableClient.RaiseEventAsync(instanceId.ToString(),
                                        ControlChannelConstants.WorkflowEvent_BuildRetentionPolicyTuples,
                                        config);
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                                }

                            case WorkflowOperation.ApplyEnvironmentRetentionPolicy:
                                {
                                    // send the appropriate signal to unblock the workflow
                                    log.LogInformation("executing operation {0}", WorkflowOperation.ApplyEnvironmentRetentionPolicy.ToString("G"));

                                    var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                oid, command.CommandCode);

                                    var config = new ActivityConfig()
                                    {
                                        ActivityContext = applianceContext.EntityState,
                                        AuthToken = impersonationToken,
                                        WorkflowOperation = command
                                    };
                                    await durableClient.RaiseEventAsync(instanceId.ToString(),
                                            ControlChannelConstants.WorkflowEvent_ApplyRetentionPolicyTuples,
                                            config);
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                                }

                            case WorkflowOperation.CommitRetentionPolicyConfiguration:
                                {
                                    // send the appropriate signal to unblock the workflow
                                    log.LogInformation("executing operation {0}", WorkflowOperation.CommitRetentionPolicyConfiguration.ToString("G"));

                                    var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                                oid, command.CommandCode);

                                    var config = new ActivityConfig()
                                    {
                                        ActivityContext = applianceContext.EntityState,
                                        AuthToken = impersonationToken,
                                        WorkflowOperation = command
                                    };
                                    await durableClient.RaiseEventAsync(instanceId.ToString(),
                                                ControlChannelConstants.WorkflowEvent_CommitRetentionPolicy,
                                                config);
                                    return await this
                                        .GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                                }
                            case WorkflowOperation.ProvisionAppliance:
                                {
                                    // provision if appliance context exists
                                    var ctx = await this.GetApplianceContextForUser(tenantId, oid, durableEntityClient);
                                    if (ctx.EntityExists)
                                    {
                                        try
                                        {
                                            await durableClient.RaiseEventAsync(instanceId.ToString(),
                                                                                ControlChannelConstants.CANCEL_WORKFLOW,
                                                                                command);

                                            httpResponseMessage = await this.GetResponseForBeginWorkflow(durableEntityClient, durableClient, starter, tenantId, oid);

                                            //var state = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, subscriptionId,
                                            //        oid, command.CommandCode);
                                            log.LogInformation("cancel signalled");
                                        }
                                        catch (Exception e)
                                        {
                                            log.LogWarning("error signalling workflow {0}", e.Message);
                                        }

                                        log.LogInformation("executing operation {0}", WorkflowOperation.ProvisionAppliance.ToString("G"));
                                        var result = await this.ProvisionDevice(durableEntityClient,
                                            tenantId, command.CandidateCommand.SubscriptionId, oid, ctx.EntityState);
                                        log.LogInformation("operation completed");
                                        return await this.GetWorkflowCheckpointResponseForUser(durableClient,
                                            durableEntityClient, tenantId, oid);
                                    }
                                    else
                                    {
                                        // trying to provision device without appliance context
                                        httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
                                        httpResponseMessage.Content = new StringContent("");
                                        return httpResponseMessage;
                                    }

                                }
                            default:
                                {
                                    log.LogWarning("failed to process workflow operation {0}", commandJson);
                                    var resp = new HttpResponseMessage();
                                    resp.StatusCode = HttpStatusCode.BadRequest;
                                    return resp;
                                }
                        }
                    }
                    else
                    {
                        // invalid workflow state transition
                        // no appliance context
                        log.LogError("invalid appliance context. command rejected {0}", commandJson);
                        var resp = new HttpResponseMessage();
                        resp.StatusCode = HttpStatusCode.NotFound;
                        return resp;
                    }

                }
                else
                {
                    log.LogWarning("dispatching command with invalid appliance context. unable to complete request");
                    httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
                    return httpResponseMessage;
                }

            }
            catch (Exception e)
            {
                // won't be anything there when the appliance is first triggered
                // or is being probed
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
                log.LogError("exception dispatching commands {0}", e.Message);
                return httpResponseMessage;
            }

            log.LogWarning("unreacheable log trace");
        }
        public async Task<HttpResponseMessage> GetResposeForPostedApplianceSessionContext(string impersonationToken, string tenantId, string oid, List<Claim> claims, IDurableClient durableClient, string commandJson)
        {
            log.LogInformation("posted command null {0}", commandJson == null);
            ApplianceSessionContextEntity applianceContext = new ApplianceSessionContextEntity();
            ApplianceInitializationResult validationResult = new ApplianceInitializationResult();

            try
            {
                log.LogInformation("deserializing context");
                applianceContext = await commandJson.FromJSONStringAsync<ApplianceSessionContextEntity>();
                log.LogInformation("done deserializing context");

            }
            catch (Exception e)
            {
                log.LogError("problem deserializing posted appliance context {0}", e.Message);
            }

            try
            {
                log.LogInformation("validating context");

                validationResult = await this.ValidateApplianceContextForUser(tenantId,
                                                oid, claims.ToList(),
                                                impersonationToken,
                                                applianceContext);
                log.LogInformation("done validating context");

            }
            catch (Exception e)
            {
                log.LogError("problem validating appliance context {0}", e.Message);
            }

            if (validationResult == ApplianceInitializationResult.SUCCEEDED)
            {
                ApplianceSessionContextEntityBase result = new ApplianceSessionContextEntityBase();


                log.LogInformation("appliance context validation succeeded. persisting context");
                try
                {

                    //// initialize current job output for each storage account
                    //foreach (var acct in await applianceContext.GetSelectedStorageAccounts())
                    //{
                    //    var newPolicy = new TableStorageRetentionPolicyEntity();

                    //    applianceContext.CurrentJobOutput.RetentionPolicyJobs.Add(new RetentionPolicyTupleContainerEntity()
                    //    {
                    //        Id = Guid.NewGuid(),
                    //        StorageAccount = acct,
                    //        SourceTuple = new Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>(
                    //          newPolicy, acct),
                    //        TableStorageRetentionPolicy = newPolicy
                    //    });
                    //}

                    result = await this.SetApplianceContextForUser(
                                        tenantId,
                                        oid,
                                        applianceContext, durableClient);
                    log.LogInformation("done persisting appliance context");
                }
                catch (Exception e)
                {
                    log.LogError("problem setting appliance context for user {0}", e.Message);
                }

                if (result.OperationResult.persistResult == AppliancePersistResultType.SUCEEDED)
                {
                    log.LogInformation("appliance context persisted");

                    try
                    {
                        log.LogInformation("provisioning device after applied appliance context");
                        var provisionResult = await this.ProvisionDevice(durableClient,
                                                tenantId, applianceContext.SelectedSubscriptionId, oid, applianceContext);
                    }
                    catch (Exception e)
                    {
                        log.LogError("problem provision device after applying appliance context {0}", e.Message);
                    }

                    log.LogInformation("appliance session context accepted");
                    var response = new HttpResponseMessage(HttpStatusCode.Accepted);
                    response.Content = new StringContent(commandJson);
                    return response;
                }
                else
                {
                    log.LogError("appliance persist failed due to {0}", result.OperationResult.ErrorMessage);
                    HttpResponseMessage badrequest = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    badrequest.Content = new StringContent(await result.ToJSONStringAsync());
                    return badrequest;
                }

                // compare the result with what was posted

            }
            else
            {

                // log.LogInformation("posted command code {0}", command.CommandCode.ToString("G"));
                HttpResponseMessage badrequest = new HttpResponseMessage(HttpStatusCode.BadRequest);
                badrequest.Content = new StringContent(commandJson);
                return badrequest;
            }
        }

        public async Task<HttpResponseMessage> GetApplianceSessionContextResponseForuser(string tenantId, string oid, IDurableEntityClient durableClient)
        {
            // handle the case of wanting to get the appliance context
            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.Accepted);
            var formatter = await this.GetJsonFormatter();
            var currentState = await this.GetApplianceContextForUser(tenantId, oid, durableClient);
            if (currentState.EntityExists == true)
            {
                var currentPolicyJobs = currentState.EntityState.CurrentJobOutput.retentionPolicyJobs;
                var metricsItems = currentPolicyJobs.SelectMany(s => s.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy.MetricRetentionSurface.MetricsRetentionSurfaceItemEntities).Count(); // .Select(s => s.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy.MetricRetentionSurface.MetricsRetentionSurfaceItemEntities).ToList().Count();
                var diagnosticsItems = currentPolicyJobs.SelectMany(s => s.TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities).Count();
                // found the entity return it
                // resp.Content = new StringContent(await currentState.EntityState.ToJSONStringAsync());
                var content = currentState.EntityState;
                resp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));
                log.LogWarning($"diagnostics retention surface {diagnosticsItems} items");
                log.LogWarning($"metrics retention surface {metricsItems} items");

                return resp;
            }
            else
            {
                resp = new HttpResponseMessage(HttpStatusCode.NotFound);
                resp.Content = new StringContent("");
                return resp;
            }
        }

        public async Task<EntityStateResponse<ApplianceSessionContextEntity>> GetApplianceContextForUser(string tenantId, string oid, IDurableEntityClient durableClient)
        {

            EntityStateResponse<ApplianceSessionContextEntity> ret = new EntityStateResponse<ApplianceSessionContextEntity>();
            try
            {
                var entityId = await this.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);

                try
                {
                    await durableClient.SignalEntityAsync<IApplianceSessionContextEntity>(entityId, proxy =>
                    { proxy.SetTimeStamp(DateTime.UtcNow); });

                }
                catch (Exception e)
                {
                    log.LogError("problem updating application context timestamp {0}", e.Message);
                }

                var currentstate = await durableClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(entityId);
                ret = currentstate;
            }
            catch (Exception e)
            {
                log.LogError("problem getting appliance context {0}", e.Message);
            }

            return ret;
        }
        /// <summary>
        /// 
        /// on failure returns uninitialized context
        /// on success returns the passed context
        /// 
        /// only call this with a validated as current
        /// tenant id and calling user oid
        /// and subscriptions in the ctx
        /// that are visible to both
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="oid"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public async Task<ApplianceSessionContextEntityBase> SetApplianceContextForUser(string tenantId, string oid, ApplianceSessionContextEntityBase ctx, IDurableClient durableEntityClient)
        {
            EntityId ctxEntitId = await GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);

            try
            {
                log.LogInformation("updating the appliance entity");
                await DeployApplianceContext(ctx, durableEntityClient, ctxEntitId);

                // avoid method side effects
                //log.LogInformation("updating workflow checkpoints for user");
                //var workflow = await this.GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantId, ctx.SelectedSubscriptionId, oid, WorkflowOperation.ProvisionAppliance);

                var persistResult = new ApplianceContextPersistResult() { persistResult = AppliancePersistResultType.SUCEEDED };
                ctx.OperationResult = persistResult;

                log.LogInformation("finished deploying new application context");
            }
            catch (Exception e)
            {
                log.LogError("problem updating appliance session context {0}", e.Message);
                // signal failure by returning an uninitialized object
                var persistResult = new ApplianceContextPersistResult()
                {
                    persistResult = AppliancePersistResultType.FAILED,
                    ErrorMessage = e.Message
                };
                ctx.OperationResult = persistResult;
                return ctx;
            }

            return ctx;
        }

        /// <summary>
        /// deploy a new appliance context
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="durableEntityClient"></param>
        /// <param name="ctxEntitId"></param>
        /// <returns></returns>
        private async Task DeployApplianceContext(ApplianceSessionContextEntityBase ctx, IDurableClient durableEntityClient, EntityId ctxEntitId)
        {

            foreach (var job in ctx.CurrentJobOutput.retentionPolicyJobs)
            {
                // init results
                job.TableStorageEntityPolicyEnforcementResult = new TableStorageEntityRetentionPolicyEnforcementResultEntity();
                job.TableStoragePolicyEnforcementResult = new TableStorageTableRetentionPolicyEnforcementResultEntity();
                // init diagnostics retention surface
                job.TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.DiagnosticsRetentionSurface.InitializeDiagnosticsRetentionSurface();
                // clear metrics retention surface
                job.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy.MetricRetentionSurface.MetricsRetentionSurfaceItemEntities.Clear();
            }

            // populate the entity's fields
            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
                proxy.SetId(ctx.Id));

            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
                proxy.SetSelectedStorageAccounts(ctx.SelectedStorageAccounts));

            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
                proxy.SetSelectedSubscriptionId(ctx.SelectedSubscriptionId));

            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
                proxy.SetTenantId(ctx.TenantId));

            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
                proxy.SetUserOid(ctx.UserOid));

            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
            {
                proxy.SetCurrentJobOutput(ctx.CurrentJobOutput);
            });


            await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxEntitId, proxy =>
            {
                proxy.SetJobOutputHistory(new List<ApplianceJobOutputEntity>());
            });
        }

        public async Task<EntityId> GetEntityIdForUser<T>(string tenantId, string oid) where T : new()
        {
            var t = new T();
            var subscriptionId = await this.GetHttpContextHeaderValueForKey(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION);
            var ctxId = CrucialExtensions.HashToGuid(tenantId, oid, subscriptionId);
            var ctxEntitId = new EntityId(t.GetType().Name, ctxId.ToString());
            return await Task.FromResult(ctxEntitId);
        }

        public TableRetentionApplianceEngine(IHttpContextAccessor ctx, AzureADJwtBearerValidation azureADJwtBearerValidation, ILogger<TableRetentionApplianceEngine> logger, ITableRetentionApplianceActivities activitiesEngine)
        {
            log = logger;

            AzureADJwtBearerValidationService = azureADJwtBearerValidation;
            this.CurrentHttpContext = ctx;
            ActivitiesEngine = activitiesEngine;
            logger.LogInformation("constructor completed");
        }


        public async Task<WorkflowCheckpoint> ProvisionDevice(IDurableEntityClient durableEntityClient, string tenantid,
                                            string subscriptionid, string userOid,
                                            ApplianceSessionContextEntityBase ctx)
        {
            WorkflowCheckpoint ret = new WorkflowCheckpoint();
            log.LogInformation("ProvisionDevice");
            log.LogTrace("subscription {0}, tenantid {0}, userid {0}", subscriptionid, tenantid, userOid);
            if (ctx != null && ctx.SelectedStorageAccounts != null)
            {
                foreach (var acct in ctx.SelectedStorageAccounts)
                {
                    log.LogInformation("appliance context set for storage account {0}", acct.Name);
                }
            }

            var operation = WorkflowOperation.ProvisionAppliance;
            var instanceId = (CrucialExtensions.HashToGuid(tenantid, userOid, subscriptionid)).ToString();

            var workflowEntityId = new EntityId(nameof(WorkflowCheckpoint), instanceId);
            var checkpointEntityId = new EntityId(nameof(WorkflowCheckpointEditMode), instanceId);
            var contextEntityId = new EntityId(nameof(ApplianceSessionContextEntity), instanceId);

            //try
            //{
            //    // reset the job output
            //    await durableEntityClient.SignalEntityAsync<IApplianceSessionContextEntity>(contextEntityId, proxy =>
            //    {
            //        proxy.SetCurrentJobOutput(new ApplianceJobOutputEntity());
            //    });
            //}
            //catch (Exception e)
            //{
            //    log.LogError("problem resetting job output");
            //}

            // TODO investigate whether this should happen or not
            //try
            //{
            //    await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(workflowEntityId,
            //                    proxy => { proxy.Delete(); });

            //    await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(checkpointEntityId,
            //                proxy => { proxy.Delete(); });
            //    log.LogInformation("deleted existing workflow checkpoint entities");
            //}
            //catch (Exception e)
            //{

            //    log.LogError("exception deleting current checkpoint steaet {0}", e.Message);
            //}

            try
            {
                log.LogInformation("resetting workflow checkopints");
                // remove the flag that causes the dashboard to show the config wizard
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(workflowEntityId, proxy =>
                {
                    proxy.SetCurrentCheckpoint(WorkflowCheckpointIdentifier.CanStartEnvironmentDiscovery);
                });

                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(checkpointEntityId, proxy =>
                {
                    proxy.SetCurrentCheckpoint(WorkflowCheckpointIdentifier.CanStartEnvironmentDiscovery);
                });
            }
            catch (Exception e)
            {
                log.LogError("problem initializing workflow checkoint {0}", e.Message);
            }
            EntityStateResponse<WorkflowCheckpoint> currentstate = await GetStateForUpdateWorkflowCheckpoints(durableEntityClient, tenantid, subscriptionid, userOid, operation);

            if ((currentstate.EntityState == null
                || (await currentstate.EntityState.GetAvailableCommands()) == null
                || (await currentstate.EntityState.GetAvailableCommands()).Count == 0))
            {
                log.LogWarning("update pending. check later");
                ret.AvailableCommands = new List<AvailableCommandEntity>()
                {
                    new AvailableCommandEntity()
                    {
                         MenuLabel = "Device Updating",
                         TenantId = tenantid,
                         UserOid = userOid,
                         SubscriptionId = subscriptionid,
                         AvailableCommandId = Guid.NewGuid().ToString(),
                         WorkflowOperation = WorkflowOperation.BeginWorkflow,
                         WorklowOperationDisplayMessage  = "Device is updating. Refresh for updated state."
                    }
                };
                ret.SubscriptionId = subscriptionid;
                ret.TimeStamp = DateTime.UtcNow;
                return ret;
            }
            else
            {
                var newCommands = await currentstate.EntityState.GetAvailableCommands();
                log.LogWarning("returning new command set {0}", await newCommands.ToJSONStringAsync());
                ret = currentstate.EntityState;
            }

            return ret;
        }

        public async Task<EntityStateResponse<WorkflowCheckpoint>> GetStateForUpdateWorkflowCheckpoints(IDurableEntityClient durableEntityClient, string tenantid, string subscriptionid, string userOid, WorkflowOperation operation)
        {
            var entityId = await this.GetEntityIdForUser<WorkflowCheckpoint>(tenantid, userOid);
            var currentstate = await durableEntityClient.ReadEntityStateAsync<WorkflowCheckpoint>(entityId);
            log.LogInformation("current workflow checkpoint state exists? {0}", currentstate.EntityExists);

            // update the live state
            log.LogInformation("update the live mode state");
            await SetWorkflowCheckpointForUser(durableEntityClient, entityId, tenantid, userOid, subscriptionid, operation);

            entityId = await this.GetEntityIdForUser<WorkflowCheckpointEditMode>(tenantid, userOid);
            log.LogInformation("update the edit mode state");
            await SetWorkflowCheckpointForUser(durableEntityClient, entityId, tenantid, userOid, subscriptionid, operation);

            // validate the write
            currentstate = await durableEntityClient.ReadEntityStateAsync<WorkflowCheckpoint>(entityId);
            return currentstate;
        }


        /// <summary>
        /// recover appliance state to initial consistentency
        /// </summary>
        /// <param name="durableEntityClient"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public async Task SetWorkflowCheckpointForUser(IDurableEntityClient durableEntityClient, EntityId entityId,
                                                    string tenantId, string userOid, string subscriptionId, WorkflowOperation workflowOperation)
        {
            //await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
            //            proxy.SetAvailableCommands(new List<AvailableCommand>() { new AvailableCommand()
            //                { WorkflowOperation = WorkflowOperation.BeginEnvironmentDiscovery, AvailableCommandId = Guid.NewGuid().ToString(),
            //                   MenuLabel = "Inventory Storage",
            //                    WorklowOperationDisplayMessage = "The appliance needs to enumerate the storage accounts in your available subscriptions" } }));

            var nextCmds = await this.GetAvailableCommandsForTransition(workflowOperation, subscriptionId, tenantId, userOid);
            await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
             proxy.SetAvailableCommands(nextCmds));

            if (workflowOperation == WorkflowOperation.ProvisionAppliance)
            {
                // TODO this should actuall be WorkflowOperation
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
                                proxy.SetCurrentCheckpoint(WorkflowCheckpointIdentifier.CanStartEnvironmentDiscovery));
            }

            // TODO to make this useful, set it to the most recent command's message
            // currently users see only command messages
            await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
                            proxy.SetMessage("manage your device"));

            await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
                            proxy.SetTimeStamp(DateTime.UtcNow));

            await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
                proxy.SetubscriptionId(subscriptionId));


        }

        public string GetAuthorizationHeader(HttpRequestHeaders req)
        {
            log.LogInformation("GetAuthorizationHeader starting");
            string ret = string.Empty;
            try
            {
                var scratchHeader = string.Empty;
                var t = new List<Tuple<string, string>>();

                foreach (var header in req)
                {
                    // log.LogInformation("provided header {0}", header.Key);
                    foreach (var v in header.Value)
                    {
                        scratchHeader += v;
                    }

                    t.Add(new Tuple<string, string>(header.Key, scratchHeader));
                    // log.LogDebug("header key" + header.Key + " value = " + header.Value);
                    scratchHeader = string.Empty;
                }
                var headerResult = t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_AUTHORIZATION)).FirstOrDefault();
                log.LogDebug("auth header present? {0}", t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_AUTHORIZATION)));

                ret = headerResult?.Item2;
                if (headerResult == null || String.IsNullOrEmpty(headerResult.Item2))
                {
                    headerResult = t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_X_ZUMO_AUTH)).FirstOrDefault();
                    ret = headerResult?.Item2;
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting authorization header {0}", e.Message);
            }

            return ret;
        }

        [Obsolete]
        public string GetAuthorizationHeaderDeprecated(HttpRequestMessage req)
        {
            log.LogInformation("GetAuthorizationHeader starting");
            string ret = string.Empty;
            try
            {
                var scratchHeader = string.Empty;
                var t = new List<Tuple<string, string>>();

                foreach (var header in req.Headers)
                {
                    // log.LogInformation("provided header {0}", header.Key);
                    foreach (var v in header.Value)
                    {
                        scratchHeader += v;
                    }

                    t.Add(new Tuple<string, string>(header.Key, scratchHeader));
                    // log.LogDebug("header key" + header.Key + " value = " + header.Value);
                    scratchHeader = string.Empty;
                }
                var headerResult = t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_AUTHORIZATION)).FirstOrDefault();
                log.LogDebug("auth header present? {0}", t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_AUTHORIZATION)));

                ret = headerResult?.Item2;
                if (headerResult == null || String.IsNullOrEmpty(headerResult.Item2))
                {
                    headerResult = t.Where(w => w.Item1.ToLower().Equals(ControlChannelConstants.HEADER_X_ZUMO_AUTH)).FirstOrDefault();
                    ret = headerResult?.Item2;
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting authorization header {0}", e.Message);
            }

            return ret;
        }

        /// <summary>
        /// this strategy defends against unprotected appliance endpoints
        /// functions should apply this strategy against injected ClaimsPrincipal
        /// AND HttpContext
        /// 
        /// strategy expects app service easy auth
        /// falls back to authorization header 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public async Task<bool> ApplyAuthorizationStrategy(HttpRequestHeaders req, ClaimsPrincipal claimsPrincipal, SubscriptionDTO subscription = null)
        {
            bool isAuthorized = false;
            try
            {
                log.LogInformation("claims principal provided? {0}", claimsPrincipal != null);
                if (claimsPrincipal != null)
                {
                    log.LogInformation("claims principal identities = {0}", claimsPrincipal.Identities.Select(s => s.Name).ToList());
                    log.LogInformation("claims = {0}", claimsPrincipal.Claims.Select(s => s.Type).ToList());

                    // your claims principal should have the name identifier claim
                    // you can make a case for something stronger
                    isAuthorized = claimsPrincipal.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_NAMEIDENTIFIER)).Any();

                    if (!isAuthorized)
                    {
                        log.LogInformation("no claims principal available. validating request authorization headers");
                        string headerValue = this.GetAuthorizationHeader(req);

                        var authResult = await this.IsAuthorized(headerValue);
                        isAuthorized = authResult.Item1;
                    }
                }
                else
                {
                    log.LogInformation("no claims principal available. validating request authorization headers");
                    string headerValue = this.GetAuthorizationHeader(req);

                    var authResult = await this.IsAuthorized(headerValue);
                    isAuthorized = authResult.Item1;
                }
            }
            catch (Exception e)
            {
                log.LogError("request failed {0}", e.Message);
            }

            return isAuthorized;
        }

        /// <summary>
        /// examines request headers for 
        /// 1) azure app service easy auth headers
        /// or
        /// 2) normal authorization headers
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task<bool> ValidateClaimsPrincipalFromHeaders(HttpRequestMessage req, SubscriptionDTO subscription = null)
        {
            bool ret = false;

            if (
                // you must post headers
                req.Headers == null
                // you may have posted a request with authorization headers
                || req.Headers.Authorization == null
                // if you assert identity to app service easy auth you must have an access token from azure ad
                // somebody could always make a case for supporting more identity providers as per azure easy auth
                || req.Headers.Where(w => w.Key.ToLowerInvariant().Contains(ControlChannelConstants.HEADER_X_MS_TOKEN_AAD_ACCESS_TOKEN)).Count() == 0
                )
            {
                log.LogError("no authorization headers provided");
                ret = false;
                return ret;
            }
            else
            {
                try
                {
                    log.LogInformation("performing authorization");
                    if (req?.Headers?.Authorization != null)
                    {
                        log.LogInformation("performing authorization header validation");

                        var authResult = await this.IsAuthorized(req.Headers.Authorization.Parameter);
                        log.LogInformation("authentication result isAuthenticated? {0}", authResult.Item1);
                        ret = authResult.Item1;
                    }
                    else
                    {
                        log.LogInformation("performing azure app service easy auth header validation");
                        if (req?.Headers?.Where(w => w.Key.ToLowerInvariant().Contains(ControlChannelConstants.HEADER_X_MS_TOKEN_AAD_ACCESS_TOKEN)).Count() > 0)
                        {
                            ret = req?.Headers?.Where(w => w.Key.ToLowerInvariant().Contains(ControlChannelConstants.HEADER_X_MS_TOKEN_AAD_ACCESS_TOKEN)).Count() > 0;
                        }
                    }

                }
                catch (Exception e)
                {
                    log.LogError("unauthorized request due to : {0}", e.Message);
                }
            }

            return ret;
        }

        /// <summary>
        /// authorize JWT tokens
        /// </summary>
        /// <param name="authorizationHeader"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, ClaimsPrincipal>> IsAuthorized(string authorizationHeader, SubscriptionDTO subscription = null)
        {

            ClaimsPrincipal principal; // This can be used for any claims
            //if ((principal = await AzureADJwtBearerValidationService.ValidateTokenAsync(authorizationHeader)) == null)
            //{
            //    return new Tuple<bool, ClaimsPrincipal>(false, principal);
            //}

            return await Task.FromResult(new Tuple<bool, ClaimsPrincipal>(true, null));

        }

        public async Task<bool> ValidateTransition(IDurableEntityClient durableClient, WorkflowOperationCommandEntity command, string tenantId,
                                                string subscriptionId, string oid)
        {
            List<AvailableCommandEntity> availableCommands = new List<AvailableCommandEntity>();

            log.LogInformation("ValidateTransition");
            var httpCtx = this.CurrentHttpContext.HttpContext;
            var entityId = await this.GetEntityIdForUser<WorkflowCheckpoint>(tenantId, oid);
            var editModeEntityId = await this.GetEntityIdForUser<WorkflowCheckpointEditMode>(tenantId, oid);

            var state = await this.GetWorkflowCheckpointEntityForUser(durableClient, entityId, tenantId, oid, subscriptionId);
            log.LogInformation("current workflow checkpoint state {0}", await state.ToJSONStringAsync());
            if (state.EntityExists == false)
            {
                var editModeState = await this.GetWorkflowCheckpointEditModeEntityForUser(durableClient, editModeEntityId, subscriptionId, tenantId, oid);
                // obsolete var editModeState = await this.GetCurrentEditModeWorkflowCheckpoint(durableClient);
                log.LogInformation("current editModeState workflow checkpoint {0}", await editModeState.ToJSONStringAsync());

                if (editModeState.EntityExists)
                {
                    log.LogInformation("edit mode state consistent");
                    availableCommands.Clear();
                    availableCommands.AddRange(editModeState.EntityState.AvailableCommands);
                    log.LogInformation("edit mode available commands count {0}", availableCommands.Count());
                }
                else
                {
                    log.LogError("inconsistent edit mode state");
                    return false;
                }
            }
            else
            {
                log.LogInformation("live workflow checkpoint state exists");
                availableCommands.Clear();
                availableCommands.AddRange(state.EntityState.AvailableCommands);
                log.LogInformation("edit mode available commands count {0}", availableCommands.Count());
            }

            var availableCommandsJson = await availableCommands.ToJSONStringAsync();
            var candidateCommandJson = await command.ToJSONStringAsync();
            // get the list of available commands
            // = await this.GetAvailableCommands(durableClient);
            log.LogInformation("current available commands {0}", availableCommandsJson);
            log.LogInformation("candidate command {0}", candidateCommandJson);
            var isValidTransition = this.IsValidTransition(command, availableCommands);
            log.LogInformation("isValidTransition {0}", isValidTransition);
            return isValidTransition;
        }

        /// <summary>
        /// is the candidate command in the list of available commands
        /// </summary>
        /// <param name="candidateCommand"></param>
        /// <param name="availableCommands"></param>
        /// <returns></returns>
        public bool IsValidTransition(WorkflowOperationCommandEntity candidateCommand, List<AvailableCommandEntity> availableCommands, SubscriptionDTO subscription = null)
        {
            var ret = false;

            // sorry you have to provide valid commands to the appliance
            if (candidateCommand != null && availableCommands != null)
            {
                // match commands that have been 'posted back' with their properties matching
                var matchesStricter = availableCommands
                                    .Where(x => x.WorkflowOperation == candidateCommand.CommandCode &&
                                           x.AvailableCommandId.Equals(candidateCommand.CandidateCommand.AvailableCommandId))
                                    .ToList<AvailableCommandEntity>();

                var matches = availableCommands
                    .Where(x => x.AvailableCommandId.Equals(candidateCommand.CandidateCommand.AvailableCommandId))
                    .ToList<AvailableCommandEntity>();
                if (matches.Count > 0)
                {
                    ret = true;
                }
            }

            return ret;
        }

        public async Task<List<AvailableCommandEntity>> GetAvailableCommandsForTransition(WorkflowOperation candidateOperation, string subscriptionId,
                                                                        string tenantid, string oid)
        {
            List<AvailableCommandEntity> ret = new List<AvailableCommandEntity>()
            {
                // initialize the default set of commands
                 new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.ProvisionAppliance, AvailableCommandId = Guid.NewGuid().ToString(),
                                MenuLabel = "Provision Appliance",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "Initializes the Appliance" },
                  new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.CancelWorkflow, AvailableCommandId = Guid.NewGuid().ToString(),
                                MenuLabel = "Cancel Workflow",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "Ends The Workflow" }
            };

            // return the default set;
            var provisionApplianceSet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.BeginWorkflow, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Begin Workflow",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "This will start the workflow on the appliance" } };

            var beginEnvironmentDiscoverySet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.GetV2StorageAccounts, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Get Storage Accounts",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "The appliance will use your authentication token to retrieve storage accounts in your subscription." } };

            var GetV2StorageAccountsSet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.BuildEnvironmentRetentionPolicy, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Build Retention Policy",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "TThe appliance will prepare metadata based on your selected storage accounts" } };


            var BuildEnvironmentRetentionPolicySet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.ApplyEnvironmentRetentionPolicy, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Apply Retention Policy",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "The appliance will calculate the retention surface" } };


            var ApplyEnvironmentRetentionPolicy = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.CommitRetentionPolicyConfiguration, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Commit Retention Policy",
                                SubscriptionId = subscriptionId,
                                TenantId = tenantid,
                                UserOid = oid,
                                WorklowOperationDisplayMessage = "The appliance will apply the retention policy to the retention surface" } };



            // emit valid commands for a given state
            switch (candidateOperation)
            {
                case WorkflowOperation.CancelWorkflow:
                    {
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.ProvisionAppliance:
                    {
                        // ret.AddRange(provisionApplianceSet);
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.BeginWorkflow:
                    {
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(provisionApplianceSet);
                        // validate the user has a token 
                        break;
                    }

                case WorkflowOperation.GetV2StorageAccounts:
                    {
                        // identify storage accounts the user has permissions on
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.BuildEnvironmentRetentionPolicy:
                    {
                        // identify tables used by microsoft azure diagnostics
                        ret.AddRange(BuildEnvironmentRetentionPolicySet);

                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.ApplyEnvironmentRetentionPolicy:
                    {
                        // identify rows for deletion
                        ret.AddRange(ApplyEnvironmentRetentionPolicy);
                        break;
                    }

                case WorkflowOperation.CommitRetentionPolicyConfiguration:
                    {
                        // the delete operation is beginning
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        ret.AddRange(ApplyEnvironmentRetentionPolicy);
                        break;
                    }
                default:
                    {
                        log.LogError("unhandled workflow transition {0}", candidateOperation);
                        break;
                    }
            }

            return await Task.FromResult<List<AvailableCommandEntity>>(ret);
        }
        [Obsolete]
        /// <summary>
        /// functions as a factory of workflow context-dependent commands 
        /// </summary>
        /// <param name="candidateOperation"></param>
        /// <returns></returns>
        public async Task<List<AvailableCommandEntity>> GetAvailableCommandsForTransition(WorkflowOperation candidateOperation, SubscriptionDTO subscription = null)
        {
            List<AvailableCommandEntity> ret = new List<AvailableCommandEntity>()
            {
                // initialize the default set of commands
                 new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.ProvisionAppliance, AvailableCommandId = Guid.NewGuid().ToString(),
                                MenuLabel = "Provision Appliance",
                                WorklowOperationDisplayMessage = "Please Initialize The Appliance" }
            };

            // return the default set;
            var provisionApplianceSet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.BeginWorkflow, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Initialize Workflow",
                                WorklowOperationDisplayMessage = "This command will reset the workflow." } };

            var beginEnvironmentDiscoverySet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.GetV2StorageAccounts, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Inventory Storage Accounts",
                                WorklowOperationDisplayMessage = "The appliance needs to enumerate the storage accounts in your available subscriptions" } };

            var GetV2StorageAccountsSet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.BuildEnvironmentRetentionPolicy, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Build Policy",
                                WorklowOperationDisplayMessage = "The appliance needs to examine your selected storage accounts to calculate the applicable policy" } };


            var BuildEnvironmentRetentionPolicySet = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.ApplyEnvironmentRetentionPolicy, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Apply Policy",
                                WorklowOperationDisplayMessage = "The appliance will verify it can apply the calculated policy" } };


            var ApplyEnvironmentRetentionPolicy = new List<AvailableCommandEntity>() { new AvailableCommandEntity()
                            { WorkflowOperation = WorkflowOperation.CommitRetentionPolicyConfiguration, AvailableCommandId = Guid.NewGuid().ToString(),
                               MenuLabel = "Commit Policy Enforcement",
                                WorklowOperationDisplayMessage = "The appliance will delete data as necessary" } };



            // emit valid commands for a given state
            switch (candidateOperation)
            {
                case WorkflowOperation.ProvisionAppliance:
                    {
                        // ret.AddRange(provisionApplianceSet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.BeginWorkflow:
                    {
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(provisionApplianceSet);
                        // validate the user has a token 
                        break;
                    }

                case WorkflowOperation.GetV2StorageAccounts:
                    {
                        // identify storage accounts the user has permissions on
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.BuildEnvironmentRetentionPolicy:
                    {
                        // identify tables used by microsoft azure diagnostics
                        ret.AddRange(BuildEnvironmentRetentionPolicySet);

                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        break;
                    }

                case WorkflowOperation.ApplyEnvironmentRetentionPolicy:
                    {
                        // identify rows for deletion
                        ret.AddRange(ApplyEnvironmentRetentionPolicy);
                        break;
                    }

                case WorkflowOperation.CommitRetentionPolicyConfiguration:
                    {
                        // the delete operation is beginning
                        ret.AddRange(beginEnvironmentDiscoverySet);
                        ret.AddRange(GetV2StorageAccountsSet);
                        ret.AddRange(provisionApplianceSet);
                        ret.AddRange(ApplyEnvironmentRetentionPolicy);
                        break;
                    }
                default:
                    {

                        break;
                    }
            }

            return await Task.FromResult<List<AvailableCommandEntity>>(ret);
        }


        public async Task<EntityStateResponse<WorkflowCheckpointEditMode>> GetWorkflowCheckpointEditModeEntityForUser(IDurableEntityClient durableClient, EntityId entityId,
                                                                         string subscriptionid,
                                                                         string tenantid,
                                                                         string oid)
        {
            var state = await durableClient.ReadEntityStateAsync<WorkflowCheckpointEditMode>(entityId);
            return state;
        }

        public async Task<EntityStateResponse<WorkflowCheckpoint>> GetWorkflowCheckpointEntityForUser(IDurableEntityClient durableClient, EntityId entityId,
                                                                                string subscriptionid,
                                                                                string tenantid,
                                                                                string oid)
        {
            var state = await durableClient.ReadEntityStateAsync<WorkflowCheckpoint>(entityId);
            return state;
        }

        public async Task<HttpResponseMessage> GetWorkflowCheckpointResponseForUser(IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid)
        {

            try
            {

                var applianceContext = await this.GetApplianceContextForUser(tenantId, oid, durableEntityClient);
                if (applianceContext.EntityExists == true)
                {
                    log.LogInformation("found appliance context for user");
                    var entityId = await this.GetEntityIdForUser<WorkflowCheckpoint>(tenantId, oid);
                    var editEntityId = await this.GetEntityIdForUser<WorkflowCheckpointEditMode>(tenantId, oid);
                    // instantiates the entity if not exists
                    await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(entityId, proxy =>
                    { proxy.SetTimeStamp(DateTime.UtcNow); });

                    await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(editEntityId, proxy =>
                    { proxy.SetTimeStamp(DateTime.UtcNow); });

                    var result = await this.GetWorkflowCheckpointEntityForUser(durableEntityClient, entityId,
                       await applianceContext.EntityState.GetSelectedSubscriptionId(), tenantId, oid);

                    log.LogInformation("getting workflow response");
                    try
                    {
                        var currentWorkflowResponse = await HandleCurrentWorkflowCheckpointResponse(durableClient, durableEntityClient, tenantId, oid, result);
                        return currentWorkflowResponse;
                    }
                    catch (Exception e)
                    {
                        log.LogError("problem getting workflow response");
                    }
                }
                else
                {
                    log.LogWarning("failed to find checkpoint for user");
                    HttpResponseMessage failResp = new HttpResponseMessage();
                    failResp.StatusCode = HttpStatusCode.NotFound;
                    return failResp;

                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting appliance context {0} for tenantid {1}, oid {2}", e.Message, tenantId, oid);
            }

            log.LogWarning("failed to find checkpoint for user");
            HttpResponseMessage resp = new HttpResponseMessage();
            resp.StatusCode = HttpStatusCode.Accepted;
            return resp;

        }

        /// <summary>
        /// manages the variable state of checkpoints
        /// during queries for their state
        /// </summary>
        /// <param name="durableClient"></param>
        /// <param name="durableEntityClient"></param>
        /// <param name="tenantId"></param>
        /// <param name="oid"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> HandleCurrentWorkflowCheckpointResponse(IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid, EntityStateResponse<WorkflowCheckpoint> result)
        {
            var formatter = await this.GetJsonFormatter();

            if (result.EntityState == null ||
                result.EntityExists != true ||
                (await result.EntityState.GetAvailableCommands()) == null ||
                (await result.EntityState.GetAvailableCommands()).Count == 0)
            {
                // TODO - commented code replaed in attempted bugfix for
                // invalid appliance state of workflow checkpoint
                //log.LogWarning("failed to find checkpoint for user");
                //HttpResponseMessage resp = new HttpResponseMessage();
                //resp.StatusCode = HttpStatusCode.NotFound;
                //return resp;
                log.LogInformation("did not find workflow checkpoint for user for user");
                HttpResponseMessage normalResp = new HttpResponseMessage();
                normalResp.StatusCode = HttpStatusCode.OK;

                try
                {
                    if (result.EntityState != null)
                    {
                        //  normalResp.Content = new StringContent(await result.EntityState.ToJSONStringAsync());

                        var content = result.EntityState;
                        normalResp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));

                    }
                }
                catch (Exception e)
                {
                    log.LogError("exception serializing workflow checkpoint response");
                }

                return normalResp;
            }
            else
            {
                // here because we can return a workflow checkpoint 
                var subscriptionId = await this.GetHttpContextHeaderValueForKey(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION);

                var instanceId = CrucialExtensions.HashToGuid(tenantId, oid, subscriptionId);
                log.LogInformation("workflow checkpoint found for user");
                var orchestrationStatus = await durableClient.GetStatusAsync(instanceId.ToString(), showHistory: true);

                if (orchestrationStatus == null)
                {
                    log.LogTrace("orchestration not running. getting checkpoint for oid {0}, tenantid {1}", oid, tenantId);

                    HttpResponseMessage normalResp = new HttpResponseMessage();
                    // normalResp.Content = new StringContent(await result.EntityState.ToJSONStringAsync());
                    normalResp.StatusCode = HttpStatusCode.OK;

                    var content = result.EntityState;
                    normalResp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));

                    return normalResp;
                }
                else if (orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Failed
                    ||
                    orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var currentEntity = result.EntityState;
                    if (orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                    {
                        var commands = await currentEntity.GetAvailableCommands();
                        var removeList = commands.Where(
                                            w => w.WorkflowOperation == WorkflowOperation.BeginWorkflow
                                        || w.WorkflowOperation == WorkflowOperation.ProvisionAppliance).ToList();

                        // mutate the outgoing results but not the stored entity
                        currentEntity.AvailableCommands = removeList;
                    }

                    else if (orchestrationStatus.RuntimeStatus == OrchestrationRuntimeStatus.Pending)
                    {
                        var commands = await currentEntity.GetAvailableCommands();
                        var removeList = commands.Where(w => w.WorkflowOperation == WorkflowOperation.CancelWorkflow).ToList();

                        // mutate the outgoing results but not the stored entity
                        currentEntity.AvailableCommands = removeList;
                        // return menu options valid for the current runtime state
                    }

                    // orchestration not running - apply the begin workflow condition
                    log.LogTrace("orchestration not running. getting checkpoint for oid {0}, tenantid {1}", oid, tenantId);

                    HttpResponseMessage normalResp = new HttpResponseMessage();
                    normalResp.StatusCode = HttpStatusCode.OK;

                    try
                    {
                        if (result.EntityState != null)
                        {
                            // normalResp.Content = new StringContent(await currentEntity.ToJSONStringAsync());

                            var content = result.EntityState;
                            normalResp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));

                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError("exception serializing workflow checkpoint response");
                    }

                    return normalResp;
                }
                else
                {
                    log.LogInformation("orchestration is running normally. returning checkpoint");
                    // here because the workflow is running normally
                    HttpResponseMessage normalResp = new HttpResponseMessage();
                    normalResp.StatusCode = HttpStatusCode.OK;

                    try
                    {
                        if (result.EntityState != null)
                        {
                            // normalResp.Content = new StringContent(await result.EntityState.ToJSONStringAsync());
                            var content = result.EntityState;
                            normalResp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));

                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError("exception serializing workflow checkpoint response");
                    }

                    return normalResp;
                }
            }
        }

        public async Task<HttpResponseMessage> GetWorkflowEditModeCheckpointResponseForUser(IDurableClient durableClient, IDurableEntityClient durableEntityClient, string tenantId, string oid)
        {
            var formatter = await this.GetJsonFormatter();
            var applianceContext = await this.GetApplianceContextForUser(tenantId, oid, durableEntityClient);
            if (applianceContext.EntityExists == true)
            {
                log.LogInformation("found checkpoint for user");
                var entityId = await this.GetEntityIdForUser<WorkflowCheckpointEditMode>(tenantId, oid);
                var result = await this.GetWorkflowCheckpointEditModeEntityForUser(durableEntityClient, entityId,
                   await applianceContext.EntityState.GetSelectedSubscriptionId(), tenantId, oid);
                if (result.EntityExists != true)
                {
                    log.LogWarning("failed to find checkpoint for user");
                    HttpResponseMessage resp = new HttpResponseMessage();
                    resp.StatusCode = HttpStatusCode.NotFound;
                    return resp;

                }
                else
                {
                    log.LogInformation("workflow checkpoint found for user");
                    HttpResponseMessage resp = new HttpResponseMessage();
                    resp.StatusCode = HttpStatusCode.OK;
                    // resp.Content = new StringContent(await result.EntityState.ToJSONStringAsync());
                    var content = result.EntityState;
                    resp.Content = new StringContent(JsonConvert.SerializeObject(content, formatter.SerializerSettings));

                    return resp;
                }
            }
            else
            {
                log.LogWarning("failed to find checkpoint for user");
                HttpResponseMessage resp = new HttpResponseMessage();
                resp.StatusCode = HttpStatusCode.NotFound;
                return resp;

            }
        }




        public JObject GetCustomEventForJObject(string policyStatus)
        {
            var wrapper = new JObject();

            JProperty eventType = new JProperty("EventType", "Azure Table Retention Policy Environment Discovery");
            wrapper.Add(eventType);

            JProperty details = new JProperty("EventDetails", policyStatus);
            wrapper.Add(details);
            return wrapper;
        }

        public async Task<OrchestrationStatusQueryResult> QueryInstancesAsync(IDurableOrchestrationClient client, OrchestrationStatusQueryCondition queryFilter)
        {
            OrchestrationStatusQueryResult result = await client.ListInstancesAsync(
                queryFilter,
                CancellationToken.None);
            foreach (DurableOrchestrationStatus instance in result.DurableOrchestrationState)
            {
                log.LogInformation("DurableOrchestrationStatus {0}", JsonConvert.SerializeObject(instance));
            }

            return result;
        }

        /// <summary>
        /// we currently implement a singleton orchestration
        /// due to the high load generated on table storage
        /// and simplicity of the management interface
        /// </summary>
        /// <param name="starter"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetResponseForBeginWorkflow([DurableClient] IDurableEntityClient entityClient,
            IDurableClient durableClient, IDurableOrchestrationClient starter, string tenantId, string oid)
        {
            DurableOrchestrationStatus durableOrchestrationStatus = new DurableOrchestrationStatus();
            var ctx = await this.GetApplianceContextForUser(tenantId, oid, entityClient);
            // reset the context

            var ctxId = await this.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);
            await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxId, proxy =>
            {
                // clear the current job output
                proxy.SetCurrentJobOutput(new ApplianceJobOutputEntity());
            });

            var instanceId = CrucialExtensions.HashToGuid(tenantId, oid);
            if (ctx.EntityExists)
            {
                var existingInstance = await starter.GetStatusAsync(instanceId.ToString());
                if ((existingInstance == null
                || (existingInstance.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                && existingInstance.RuntimeStatus != OrchestrationRuntimeStatus.Pending))
                {
                    // An instance with the specified ID doesn't exist or an existing one stopped running, create one.
                    // await starter.StartNewAsync(functionName, instanceId, eventData);
                    // Function input comes from the request content.
                    string startedInstanceId = await starter.StartNewAsync(ControlChannelConstants.WorkflowEntrypointDebug,
                        instanceId.ToString(), ctx.EntityState);
                    durableOrchestrationStatus = await starter.GetStatusAsync(startedInstanceId);
                    var newstate = await this.GetStateForUpdateWorkflowCheckpoints(entityClient, tenantId, ctx.EntityState.SelectedSubscriptionId, oid, WorkflowOperation.BeginWorkflow);
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
                    log.LogInformation($"Started orchestration with ID = '{startedInstanceId}'.");
                    return resp;



                }
                else
                {
                    /// TODO here 
                    /// manage the fact that our singleton is not really a singleton anymore
                    ///                 log.LogWarning("orchesdtration cannot start without appliance context");
                    //string startedInstanceId = await starter.StartNewAsync(ControlChannelConstants.WorkflowEntrypointDebug,
                    //instanceId.ToString(), ctx.EntityState);
                    var newstate = await this.GetStateForUpdateWorkflowCheckpoints(entityClient, tenantId, ctx.EntityState.SelectedSubscriptionId, oid, WorkflowOperation.BeginWorkflow);
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
                    return resp;
                }
            }
            else
            {
                log.LogWarning("orchesdtration cannot start without appliance context");
                HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.NotFound);
                return resp;
            }


            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(durableOrchestrationStatus),
                                    Encoding.UTF8,
                                    "application/json")
            };
            return httpResponseMessage;
        }

        public async Task<bool> SetCurrentJobOutput(string tenantId, string oid, ApplianceSessionContextEntity ctx, IDurableClient durableClient)
        {
            try
            {
                var ctxId = await this.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);
                await durableClient.SignalEntityAsync<IApplianceSessionContextEntity>(ctxId, proxy =>
                {
                // clear the current job output
                proxy.SetCurrentJobOutput(ctx.CurrentJobOutput);
                });
            }
            catch(Exception e)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> SetWorkflowState(IDurableClient durableEntityClient,
                                List<AvailableCommandEntity> commands,
                                string message,
                                WorkflowCheckpointIdentifier commandCode, SubscriptionDTO subscription = null)
        {
            bool ret = false;
            var editid = new EntityId(nameof(WorkflowCheckpointEditMode),
                            ControlChannelConstants.DefaultWorkflowCheckpointEntityKey);
            var liveid = new EntityId(nameof(WorkflowCheckpointEditMode),
                ControlChannelConstants.DefaultWorkflowCheckpointEntityKey);
            try
            {
                log.LogInformation("updating appliance edit mode state");
                // update the edit mode
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(editid, proxy =>
                            proxy.SetAvailableCommands(commands));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(editid, proxy =>
                      proxy.SetTimeStamp(DateTime.UtcNow));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(editid, proxy =>
                    proxy.SetCurrentCheckpoint(commandCode));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(editid, proxy =>
                                proxy.SetMessage(message));

                log.LogInformation("updating appliance live mode state");
                // update the live mode
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(liveid, proxy =>
                proxy.SetAvailableCommands(commands));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(liveid, proxy =>
                      proxy.SetTimeStamp(DateTime.UtcNow));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(liveid, proxy =>
                    proxy.SetCurrentCheckpoint(commandCode));
                await durableEntityClient.SignalEntityAsync<IWorkflowCheckpoint>(liveid, proxy =>
                                proxy.SetMessage(message));

                ret = true;
            }
            catch (Exception e)
            {
                log.LogError("unable to update appliance state due to {0}", e.Message);
            }

            log.LogInformation("appliance state successfully updated? {0}", ret);
            return ret;
        }

        public Task<TableStorageRetentionPolicyEntity> GetRetentionPolicy(string tenantId, string subscriptionId, string storageAccountId, string oid, IDurableClient durableClient)
        {
            throw new NotImplementedException();
        }

        public Task<TableStorageRetentionPolicyEntity> SetGetRetentionPolicy(string tenantId, string subscriptionId, string storageAccountId, string oid, TableStorageRetentionPolicyEntity policy, IDurableClient durableClient)
        {
            throw new NotImplementedException();
        }

        public async Task Log(JobOutputLogEntry logEntry, string tenantId, string oid, IDurableEntityClient entityClient)
        {
            try
            {
                StackTrace stackTrace = new StackTrace();

                var methods = stackTrace.GetFrames().Select(s => s.GetMethod()).ToList();
                var filtered = methods.Select(s => s.DeclaringType).ToList();

                var frames = filtered.Where( w => w != null &&  w.FullName != null && w.FullName.Contains("ataxlab")).ToList(); // stackTrace.GetFrames().Where(w => w.GetMethod().DeclaringType.FullName.ToLower().Contains("ataxlab".ToLower())).ToList();
                
                
                logEntry.source = frames[2].Name ; //stackTrace.GetFrame(2).GetMethod().Name;

                var entityId = await this.GetEntityIdForUser<JobOutputLogEntity>(tenantId, oid);
                await entityClient.SignalEntityAsync<IJobOutputLogEntity>(entityId, proxy =>
                {
                    proxy.appendLog(logEntry);
                    log.LogInformation("operator log updated");
                });
            }
            catch (Exception e)
            {
                log.LogError($"exception writing appliance job log: {e.Message}");
            }

        }

        private async Task<JsonMediaTypeFormatter> GetJsonFormatter()
        {
            return await Task.FromResult(
                new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                    ,
                    UseDataContractJsonSerializer = false
                }

                );
        }
    }
}
