using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.extensions;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.state.entities;
using com.ataxlab.functions.table.retention.services;
using com.ataxlab.functions.table.retention.utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.c2
{
    public class ControlChannelFunctions
    {
        //private IConfiguration _configuration;
        private ITableRetentionApplianceEngine TableRetentionApplianceEngine { get; set; }

        ILogger<ControlChannelFunctions> log;
        private IHttpContextAccessor CurrentHttpContext { get; set; }
        /// <summary>
        /// class globally available authentication status
        /// as we do jwt validation
        /// due to the variance in authority sts within
        /// a single azure tenant we do not currently validate (have a tested heurestic)
        /// the issuer endpoint but its other details
        /// </summary>
        bool IsAuthorized { get; set; }

        public ControlChannelFunctions(IHttpContextAccessor ctx, ITableRetentionApplianceEngine engine,
                                        //IConfiguration configuration, 
                                        ILogger<ControlChannelFunctions> logger)
        {

            //_configuration = configuration;
            TableRetentionApplianceEngine = engine;

            this.CurrentHttpContext = ctx;
            log = logger;
        }

        [FunctionName(ControlChannelConstants.ApplianceContextEndpoint)]
        public async Task<HttpResponseMessage> ApplianceContext(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = ControlChannelConstants.ApplianceContextEndpoint +
                                                                         ControlChannelConstants.ApplianceContextRouteTemplate)] HttpRequestMessage req,
        string tenantId,
        string oid,
        ClaimsPrincipal user,
        [DurableClient] IDurableClient durableClient,
        ILogger log)
        {
            log.LogInformation("appliance context endpoint is handling a request.");
            try
            {
                var httpCtx = this.CurrentHttpContext.HttpContext;

                // handle the case of sending a new appliance context
                if (req.Method == HttpMethod.Post)
                {
                    var commandJson = await req.Content.ReadAsStringAsync(); // await HttpRequestHelper.DeserializeHttpMessageBody<WorkflowOperationCommand>(req);
                    if (!String.IsNullOrEmpty(commandJson))
                    {
                        var headers = req.Headers;

                        string impersonationToken = await this.TableRetentionApplianceEngine.GetImpersonationTokenFromHeaders(headers);
                        var response = await this.TableRetentionApplianceEngine.
                                        GetResposeForPostedApplianceSessionContext(impersonationToken,
                                        tenantId, oid, user.Claims.ToList(), durableClient, commandJson);
                        log.LogInformation("handled posted appliancesessioncontext. returing response");
                        return response;
                    }
                    else
                    {
                        log.LogWarning("invalid appliance context posted");
                        HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        resp.Content = new StringContent("");
                        return resp;
                    }

                }
                else if (req.Method == HttpMethod.Get)
                {
                    log.LogInformation("handling get request for appliance session context");
                    return await this.TableRetentionApplianceEngine.GetApplianceSessionContextResponseForuser(tenantId, oid, durableClient);
                }
                else
                {

                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.NotFound);
                    resp.Content = new StringContent("");
                    return resp;
                }
            }
            catch (Exception e)
            {
                log.LogError("problem handling applied appliance context {0}", e.Message);
                HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                resp.Content = new StringContent("");
                return resp;
            }
        }



        [FunctionName(ControlChannelConstants.ApplicationControlChannelEndpoint)]
        public async Task<HttpResponseMessage> WorkflowOperator(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = ControlChannelConstants.ApplicationControlChannelEndpoint
                                                                     + ControlChannelConstants.ApplianceContextRouteTemplate)] HttpRequestMessage req,
           [DurableClient] IDurableOrchestrationClient starter,
            Microsoft.Azure.WebJobs.ExecutionContext context,
           [DurableClient] IDurableClient durableClient,
           [DurableClient] IDurableEntityClient durableEntityClient,
            ClaimsPrincipal claimsPrincipal,
            string tenantId,
            string oid, 
            ILogger log)
        {
            log.LogInformation("ApplicationControlChannelEndpoint HttpRequest {0}", req.RequestUri.AbsoluteUri);
            var commandJson = await req.Content.ReadAsStringAsync();

            bool isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);
            var impersonate = await this.TableRetentionApplianceEngine.GetImpersonationTokenFromHeaders(req.Headers);
            log.LogInformation("WorkflowOperator Operation Authorized? {0}", isAuthorized);
            var response = await this.TableRetentionApplianceEngine.GetResponseForWorkflowOperator(starter, durableClient, durableEntityClient, tenantId, oid, commandJson, impersonate);
            log.LogInformation("returning workflow operator endpoint response");
            return response;
        }


        [FunctionName(ControlChannelConstants.ResetWorkflowsEndpoint)]
        public async Task<HttpResponseMessage> PurgeWorkflows(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = ControlChannelConstants.ResetWorkflowsEndpoint)] HttpRequestMessage req,
       [DurableClient] IDurableOrchestrationClient client,
        ClaimsPrincipal claimsPrincipal,
        ILogger log)
        {

            log.LogInformation("ResetWorkflowsEndpoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);
            OrchestrationStatusQueryResult result = await DeleteRunningOrchestrationEntities(client);

            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(result))
            };

            return httpResponseMessage;
        }

        private async Task<OrchestrationStatusQueryResult> DeleteRunningOrchestrationEntities(IDurableOrchestrationClient client)
        {
            string reason = "deleted by operator at " + DateTime.UtcNow.ToLongTimeString();
            // Get the first 100 running or pending instances that were created between 7 and 1 day(s) ago
            var queryFilter = new OrchestrationStatusQueryCondition
            {
                RuntimeStatus = new[]
                {
            OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running, OrchestrationRuntimeStatus.ContinuedAsNew
                },
                CreatedTimeFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(999)),
                CreatedTimeTo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                PageSize = 100,
            };

            OrchestrationStatusQueryResult result = new OrchestrationStatusQueryResult();
            try
            {
                result = await TableRetentionApplianceEngine.QueryInstancesAsync(client, queryFilter);
                foreach (var instance in result.DurableOrchestrationState)
                {
                    try
                    {
                        await client.TerminateAsync(instance.InstanceId, reason);
                    }
                    catch (Exception e)
                    {
                        log.LogError("problem deleting instances {0}", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError("problem deleting instances {0}", e.Message);
            }

            return result;
        }

        [FunctionName(ControlChannelConstants.QueryOrchestrationStatusEndpoint)]
        public async Task<List<DurableOrchestrationStateDTO>> QueryStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ControlChannelConstants.QueryOrchestrationStatusEndpoint
                                                                     + ControlChannelConstants.QueryOrchestrationStatusRouteTemplate)] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient client,
        [DurableClient] IDurableEntityClient enityClient,
        [DurableClient] IDurableClient durableClient,
         ClaimsPrincipal claimsPrincipal,
         string tenantId,
         string oid, int fromDays,
         ILogger log)
        {
            log.LogInformation("QueryWorkflowsStatusEndpoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);
            var noFilter = new OrchestrationStatusQueryCondition();
            //OrchestrationStatusQueryResult result = await client.ListInstancesAsync(
            //    noFilter,
            //    CancellationToken.None);

            // Get the first 100 running or pending instances that were created between 7 and 1 day(s) ago
            var queryFilter = new OrchestrationStatusQueryCondition
            {
                RuntimeStatus = new[]
                {
                            OrchestrationRuntimeStatus.ContinuedAsNew,
                            OrchestrationRuntimeStatus.Pending,
                            OrchestrationRuntimeStatus.Running
                },
                // TODO - implement httpmethod parameters for these properties
                CreatedTimeFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(fromDays)),
                CreatedTimeTo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                PageSize = 100,
            };

            var result = new List<DurableOrchestrationStatus>();
            var ret = new List<DurableOrchestrationStateDTO>();

            // get the orchestration state
            try
            {
                OrchestrationStatusQueryResult state = await TableRetentionApplianceEngine.QueryInstancesAsync(client, queryFilter);

                if (state != null && state.DurableOrchestrationState != null)
                {
                    try
                    {
                        log.LogInformation("getting orchestration state");

                        foreach (var instance in state.DurableOrchestrationState)
                        {
                            try
                            {
                                result.Add(instance);
                            }
                            catch (Exception e)
                            {
                                log.LogError("issue getting orchestration state: {0}", e.Message);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        log.LogError("problem getting durable orchestration state {0}", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting orchestration instances {0}", e.Message);
            }

            var subscriptionId = await this.TableRetentionApplianceEngine.GetHttpContextHeaderValueForKey(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION);
            var instanceId = CrucialExtensions.HashToGuid(tenantId, oid, subscriptionId);
            //var workflowCheckpointEntityId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<WorkflowCheckpoint>(tenantId, oid);
            //var workflowCheckpointEditModeEntityid = await this.TableRetentionApplianceEngine.GetEntityIdForUser<WorkflowCheckpointEditMode>(tenantId, oid);
            //var applianceContext = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);


            //var subscriptionId = applianceContext.EntityState.SelectedSubscriptionId;
            //var editModeEntity = await this.TableRetentionApplianceEngine.GetWorkflowCheckpointEditModeEntityForUser(durableClient,
            //   workflowCheckpointEditModeEntityid, subscriptionId, tenantId, oid);


            // add some we know about

            try
            {
                var existingInstance = await client.GetStatusAsync(instanceId.ToString());
                if (existingInstance != null && !ret.Where(w => w.instanceId == existingInstance.InstanceId).Any())
                {
                    ret.Add(new DurableOrchestrationStateDTO()
                    {
                        createdTime = existingInstance.CreatedTime,
                        customStatus = existingInstance.CustomStatus == null ? String.Empty :
                                        await existingInstance.CustomStatus.ToJSONStringAsync(),
                        instanceId = existingInstance.InstanceId,
                        name = existingInstance.Name,
                        runtimeStatus = existingInstance.RuntimeStatus,
                        lastUpdatedTime = existingInstance.LastUpdatedTime,
                        history = existingInstance.History == new Newtonsoft.Json.Linq.JArray() ? null :
                                    await existingInstance.History.ToJSONStringAsync(),
                        input = existingInstance.Input == null ? String.Empty :
                                await existingInstance.Input.ToJSONStringAsync(),
                        output = existingInstance.Output == null ? String.Empty :
                                await existingInstance.Output.ToJSONStringAsync()
                    });
                }
            }
            catch (Exception e)
            {
                log.LogError("issue getting status {0}", e.Message);
            }

            foreach (var r in result)
            {
                // todo - enable filtering for the calling user
                try
                {
                    ret.Add(new DurableOrchestrationStateDTO()
                    {
                        createdTime = r.CreatedTime,
                        customStatus = r.CustomStatus == null ? "" : await r.CustomStatus.ToJSONStringAsync(),
                        instanceId = r.InstanceId,
                        name = r.Name,
                        runtimeStatus = r.RuntimeStatus,
                        lastUpdatedTime = r.LastUpdatedTime,
                        history = r.History == null ? "" : await r.History.ToJSONStringAsync(),
                        input = r.Input == null ? "" : await r.Input.ToJSONStringAsync(),
                        output = r.Output == null ? "" : await r.Output.ToJSONStringAsync()
                    }
                    );
                }
                catch (Exception e)
                {
                    log.LogError("issue getting orchestrations {0}", e.Message);
                }
            }

            return ret;
        }



        [FunctionName(ControlChannelConstants.AbandonOrchestrationsEndpoint)]
        public async Task<OrchestrationStatusQueryResult> AbandonOrchestrations([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = ControlChannelConstants.AbandonOrchestrationsEndpoint)] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient client,
         ClaimsPrincipal claimsPrincipal,
         ILogger log)
        {
            log.LogInformation("AbandonOrchestrationsEndpoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);

            // dequeue the command
            var commandset = await CrucialExtensions.DeserializeHttpMessageBody<OrchestrationStatusQueryResult>(req);
            if (commandset != null && commandset.DurableOrchestrationState?.Count() > 0)
            {
                try
                {
                    foreach (var orchestration in commandset.DurableOrchestrationState)
                    {
                        // delete the orchestration 
                        await client.TerminateAsync(orchestration.InstanceId, "operator terminated");

                    }
                }
                catch (Exception e)
                {
                    log.LogError("issue terminating {0}", e.Message);
                }
            }

            // Get the first 100 running or pending instances that were created between 7 and 1 day(s) ago
            var queryFilter = new OrchestrationStatusQueryCondition
            {
                RuntimeStatus = new[]
                {
                    OrchestrationRuntimeStatus.Running , OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Completed, OrchestrationRuntimeStatus.Canceled
                },
                CreatedTimeFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)),
                CreatedTimeTo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                PageSize = 100,
            };

            OrchestrationStatusQueryResult result = await TableRetentionApplianceEngine.QueryInstancesAsync(client, queryFilter);

            return result;
        }

        /// <summary>
        /// data store utility function
        /// </summary>
        /// <param name="req"></param>
        /// <param name="client"></param>

        /// <returns></returns>
        [FunctionName(ControlChannelConstants.DeleteWorkflowCheckpointEditModeEndPoint)]
        public async Task<HttpResponseMessage> DeleteWorkflowCheckpointEditMode(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = ControlChannelConstants.DeleteWorkflowCheckpointEditModeEndPoint
                                                                     + ControlChannelConstants.DeleteWorkflowCheckpointEditModeRouteTemplate)] HttpRequestMessage req,
       [DurableClient] IDurableEntityClient client,
       [DurableClient] IDurableOrchestrationClient orchestrationClient,
       string tenantId,
       string oid,
        ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("DeleteWorkflowCheckpointEditModeEndPoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);

            log.LogInformation("Delete Operation Authorized? {0}", isAuthorized);
            //provisionally ignore the entity key
            var subscriptionId = await this.TableRetentionApplianceEngine.GetHttpContextHeaderValueForKey(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION);

            string entityKey = (CrucialExtensions.HashToGuid(tenantId, oid, subscriptionId)).ToString();


            // delete running orchestrations
            var deletedOrchestrations = await DeleteRunningOrchestrationEntities(orchestrationClient);

            // delete associated durable entities
            var workflowCheckpointEntityId = new EntityId(nameof(WorkflowCheckpoint), entityKey);
            var editModeEntityId = new EntityId(nameof(WorkflowCheckpointEditMode), entityKey);
            var contextKey = new EntityId(nameof(ApplianceSessionContextEntity), entityKey);

            log.LogInformation("configured entity keys");
            await client.SignalEntityAsync(workflowCheckpointEntityId, "Delete");
            await client.SignalEntityAsync(editModeEntityId, "Delete");
            await client.SignalEntityAsync(contextKey, "Delete");


            log.LogInformation("signalled entities");
            OrchestrationStatusQueryResult deleteResult = new OrchestrationStatusQueryResult();
            try
            {
                log.LogInformation("deleting orchestration entities");
                deleteResult = await DeleteRunningOrchestrationEntities(orchestrationClient);
                log.LogInformation("current orchestration status {0}", await deleteResult.DurableOrchestrationState.ToJSONStringAsync());
            }
            catch (Exception e)
            {
                log.LogError("issue deleting running orchestrations {0}", e.Message);
            }
            // init the associated entities
            await client.SignalEntityAsync<IWorkflowCheckpoint>(workflowCheckpointEntityId, proxy =>
            {
                proxy.SetTimeStamp(DateTime.UtcNow);
            });

            await client.SignalEntityAsync<IWorkflowCheckpoint>(workflowCheckpointEntityId, proxy =>
            {
                proxy.SetCurrentCheckpoint(WorkflowCheckpointIdentifier.UnProvisioned);
            });

            await client.SignalEntityAsync<IWorkflowCheckpoint>(workflowCheckpointEntityId, proxy =>
            {
                proxy.SetMessage("device is in factory reset condition");
            });


            // init the associated entities
            await client.SignalEntityAsync<IWorkflowCheckpoint>(editModeEntityId, proxy =>
            {
                proxy.SetTimeStamp(DateTime.UtcNow);
            });

            await client.SignalEntityAsync<IWorkflowCheckpoint>(editModeEntityId, proxy =>
            {
                proxy.SetCurrentCheckpoint(WorkflowCheckpointIdentifier.UnProvisioned);
            });

            await client.SignalEntityAsync<IWorkflowCheckpoint>(editModeEntityId, proxy =>
            {
                proxy.SetMessage("device is in factory reset condition");
            });

            // ensure the default is deleted - might be double write in some cases

            // reinitialize durable entities
            // var initializedCheckpoint = await this.TableRetentionApplianceEngine.ProvisionDevice(client);
            //return req.CreateResponse<IEnumerable<DurableOrchestrationStatus>>(HttpStatusCode.Accepted, deleteResult.DurableOrchestrationState);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// reports the most recent checkpoint
        /// set by the orchestration
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="durableClient"></param>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName(ControlChannelConstants.QueryWorkflowEditModeCheckpointStatusEndpoint)]
        public async Task<HttpResponseMessage> GetCurrentWorkflowEditModeCheckpoint(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ControlChannelConstants.QueryWorkflowEditModeCheckpointStatusEndpoint
                                                                     + ControlChannelConstants.QueryWorkflowEditModeCheckpointStatusRouteTemplate)]
           HttpRequestMessage req,
         [DurableClient] IDurableClient durableClient,
         [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
          ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("QueryWorkflowEditModeCheckpointStatusEndpoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);
            if (isAuthorized)
            {
                var response = await this.TableRetentionApplianceEngine.GetWorkflowEditModeCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                return response;
            }
            else
            {
                // fell through to here because of unauthorized request
                HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                unauthorizedResp.StatusCode = HttpStatusCode.Unauthorized;
                return unauthorizedResp;

            }

        }


        /// <summary>
        /// reports the most recent checkpoint
        /// set by the orchestration
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="durableClient"></param>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName(ControlChannelConstants.QueryWorkflowCheckpointStatusEndpoint)]
        public async Task<HttpResponseMessage> GetCurrentWorkflowCheckpoint(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ControlChannelConstants.QueryWorkflowCheckpointStatusEndpoint
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
           HttpRequestMessage req,
         [DurableClient] IDurableClient durableClient,
         [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
          ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("QueryWorkflowCheckpointStatusEndpoint");
            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);

            //try
            //{
            //    var scratchHeader = string.Empty;
            //    var t = new List<Tuple<string, string>>();

            //    foreach (var header in req.Headers)
            //    {
            //        // log.LogInformation("provided header {0}", header.Key);
            //        foreach (var v in header.Value)
            //        {
            //            scratchHeader += v;
            //        }

            //        t.Add(new Tuple<string, string>(header.Key, scratchHeader));
            //        // log.LogDebug("header key" + header.Key + " value = " + header.Value.FirstOrDefault());
            //        scratchHeader = string.Empty;
            //    }


            //    var impersonationToken = req.Headers.Where(w => w.Key.Equals(ControlChannelConstants.HEADER_IMPERSONATION_TOKEN)).Select(s => s.Value)?.FirstOrDefault();
            //    var impersonate = impersonationToken?.FirstOrDefault()?.TrimStart(',')?.Trim();
            //    // the subscription id header
            //    //var aSubscription = req.Headers.Where(w => w.Key.Equals(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)).Select(s => s.Value)?.FirstOrDefault();
            //    //var theSubscriptionId = aSubscription?.FirstOrDefault()?.TrimStart(',')?.Trim();
            //    // the token acquired by exchange of easy auth provider token, id's the appliance
            //    var clientPrincipalId = req.Headers.Where(w => w.Key.Equals("X-MS-CLIENT-PRINCIPAL-ID")).Select(s => s.Value)?.FirstOrDefault();//. .Select(s => s.Value).FirstOrDefault();
            //    log.LogInformation("provided header X-MS-ClIENT-PRINCIPAL-ID = {0}", clientPrincipalId?.FirstOrDefault());

            //    // the token acquired by exchange of easy auth provider token, id's the appliance
            //    // missing outside of azure with easyauth enabled
            //    var idToken = req.Headers.Where(w => w.Key.Equals("X-MS-TOKEN-AAD-ID-TOKEN"))?.Select(s => s.Value)?.FirstOrDefault();
            //    log.LogInformation("provided header X-MS-TOKEN-AAD-ID-TOKEN = {0}", idToken);


            //    // the token acquired by exchange of easy auth provider token, id's the appliance
            //    var accessToken = req.Headers.Where(w => w.Key.Equals("X-MS-TOKEN-AAD-ACCESS-TOKEN")).Select(s => s.Value)?.FirstOrDefault();
            //    log.LogInformation("provided header X-MS-TOKEN-AAD-ACCESS-TOKEN = {0}", accessToken);

            //    // The Easy Auth Provider token, id's the user
            //    var zumoToken = req.Headers.Where(w => w.Key.Equals(ControlChannelConstants.HEADER_X_ZUMO_AUTH)).Select(s => s.Value)?.FirstOrDefault();
            //    log.LogInformation("provided header X-ZUMO-AUTH = {0}", zumoToken);

            //    IEnumerable<string> userToken;
            //    bool storageAccessResult = false;

            //    //try
            //    //{
            //    //    log.LogInformation("testing X-MS-TOKEN-AAD-ACCESS-TOKEN");
            //    //    storageAccessResult = await ValidateAccessTokenForStorage(accessToken);
            //    //    log.LogInformation("testing xms-token-aad-access-token result {0}", storageAccessResult);
            //    //}
            //    //catch (Exception e) { }

            //    //try
            //    //{
            //    //    storageAccessResult = await ValidateAccessTokenForStorage(zumoToken);
            //    //    log.LogInformation("testing zumoToken-token result {0}", storageAccessResult);
            //    //}
            //    //catch (Exception e) { }

            //    //try
            //    //{
            //    //    storageAccessResult = await ValidateAccessTokenForStorage(idToken);
            //    //    log.LogInformation("testing idToken result {0}", storageAccessResult);
            //    //}
            //    //catch (Exception e) { }
            //}
            //catch (Exception e)
            //{
            //    log.LogError("storage account experiment failed {0}", e.Message);
            //}


            if (isAuthorized)
            {
                log.LogInformation("authorized request");
                try
                {
                    log.LogInformation("getting workflow checkpoint response for user");
                    var response = await this.TableRetentionApplianceEngine.GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                    log.LogInformation("got workflow checkpoint response for user");

                    return response;
                }
                catch (Exception e)
                {

                    log.LogWarning("problem getting checkpoint for user. recovering state {0}", e.Message);

                    log.LogError("problem getting checkpoint {0}", e.Message);
                    HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                    unauthorizedResp.StatusCode = HttpStatusCode.Accepted;
                    return unauthorizedResp;
                }
            }
            else
            {

                log.LogWarning("unauthorized request");
                // fell through to here because of unauthorized request
                HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                unauthorizedResp.StatusCode = HttpStatusCode.Unauthorized;
                return unauthorizedResp;

            }

        }


    }
}
