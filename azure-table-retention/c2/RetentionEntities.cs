using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.extensions;
using com.ataxlab.azure.table.retention.state.entities;
using com.ataxlab.functions.table.retention.entities;
using com.ataxlab.functions.table.retention.services;
using com.ataxlab.functions.table.retention.utility;
using Microsoft.AspNetCore.Mvc;
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
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.c2
{


    /// <summary>
    /// functions that emit raw json entities
    /// </summary>
    [Route("RetentionEntities")]
    public class RetentionEntities
    {


        private ITableRetentionApplianceEngine TableRetentionApplianceEngine { get; set; }

        private const string CONTENT_TYPE_APPLICATION_JSON = "application/json";
        ILogger<RetentionEntities> log;
        public RetentionEntities(ITableRetentionApplianceEngine engine,
            ILogger<RetentionEntities> logger)
        {
            TableRetentionApplianceEngine = engine;


            log = logger;
        }

        [FunctionName(ControlChannelConstants.RetentionPolicyPostEndpoint)]
        //[OpenApiOperation(operationId: ControlChannelConstants.RetentionPolicyPostEndpoint, tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "tenantId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **tenantId** parameter is necessary")]
        //[OpenApiParameter(name: "oid", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **oid** parameter is necessary")]
        //[OpenApiParameter(name: "subscriptionId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **subscriptionId** parameter is necessary")]
        //[OpenApiParameter(name: "storageAccountId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **storageAccountId** parameter is necessary")]
        //[OpenApiRequestBody(contentType: CONTENT_TYPE_APPLICATION_JSON, bodyType: typeof(TableStorageRetentionPolicyEntity))]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: CONTENT_TYPE_APPLICATION_JSON, bodyType: typeof(TableStorageRetentionPolicyEntity), Description = "The OK response")]
        [Obsolete]
        public async Task<TableStorageRetentionPolicyEntity> PostRetentionPolicyEndpoint(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post" , Route = ControlChannelConstants.RetentionPolicyPostEndpoint
                                                            + ControlChannelConstants.RetentionPolicyRouteTemplate)]
            HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
            string subscriptionId,
            string storageAccountId,
            ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("RetentionPolicyEndpoint");
            var ret = new TableStorageRetentionPolicyEntity();

            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);

            if (isAuthorized)
            {
                log.LogInformation("authorized request");
                try
                {
                    log.LogInformation("getting workflow checkpoint response for user");
                    var response = await this.TableRetentionApplianceEngine.GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                    log.LogInformation("got workflow checkpoint response for user");


                    return ret;
                }
                catch (Exception e)
                {

                    log.LogWarning("problem getting checkpoint for user. recovering state {0}", e.Message);

                    log.LogError("problem getting checkpoint {0}", e.Message);
                    HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                    unauthorizedResp.StatusCode = HttpStatusCode.Accepted;

                    return ret;
                }
            }
            else
            {

                log.LogWarning("unauthorized request");
                // fell through to here because of unauthorized request
                HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                unauthorizedResp.StatusCode = HttpStatusCode.Unauthorized;

                return ret;

            }
        }


        [FunctionName(ControlChannelConstants.RetentionPolicyEndpoint)]
        //[OpenApiOperation(operationId: ControlChannelConstants.RetentionPolicyEndpoint, tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "tenantId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **tenantId** parameter is necessary")]
        //[OpenApiParameter(name: "oid", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **oid** parameter is necessary")]
        //[OpenApiParameter(name: "subscriptionId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **subscriptionId** parameter is necessary")]
        //[OpenApiParameter(name: "storageAccountId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **storageAccountId** parameter is necessary")]
        // [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: CONTENT_TYPE_APPLICATION_JSON, bodyType: typeof(TableStorageRetentionPolicyEntity), Description = "The OK response")]

        public async Task<TableStorageRetentionPolicyEntity> GetRetentionPolicyEndpoint(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get" , Route = ControlChannelConstants.RetentionPolicyEndpoint
                                                            + ControlChannelConstants.RetentionPolicyRouteTemplate)]
        HttpRequestMessage req,
        [DurableClient] IDurableClient durableClient,
        [DurableClient] IDurableEntityClient durableEntityClient,
        string tenantId,
        string oid,
        string subscriptionId,
        string storageAccountId,
        ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("RetentionPolicyEndpoint");
            var ret = new TableStorageRetentionPolicyEntity();

            bool isAuthorized = false;
            isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);

            if (isAuthorized)
            {
                log.LogInformation("authorized request");
                try
                {
                    log.LogInformation("getting workflow checkpoint response for user");
                    var response = await this.TableRetentionApplianceEngine.GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
                    log.LogInformation("got workflow checkpoint response for user");

                    return ret;
                }
                catch (Exception e)
                {

                    log.LogWarning("problem getting checkpoint for user. recovering state {0}", e.Message);

                    log.LogError("problem getting checkpoint {0}", e.Message);
                    HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                    unauthorizedResp.StatusCode = HttpStatusCode.Accepted;

                    return ret;
                }
            }
            else
            {

                log.LogWarning("unauthorized request");
                // fell through to here because of unauthorized request
                HttpResponseMessage unauthorizedResp = new HttpResponseMessage();
                unauthorizedResp.StatusCode = HttpStatusCode.Unauthorized;
                return ret;

            }
        }


        [FunctionName("GetWorkflowCheckpoint")]
        public async Task<HttpResponseMessage> GetWorkflowCheckpoint([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetWorkflowCheckpoint"
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
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
                var response = await this.TableRetentionApplianceEngine.GetWorkflowCheckpointResponseForUser(durableClient, durableEntityClient, tenantId, oid);
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

        [FunctionName("GetApplianceSessionContext")]
        public async Task<HttpResponseMessage> GetApplianceSessionContext(

        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetApplianceSessionContext" +
                                                                         ControlChannelConstants.ApplianceContextRouteTemplate)] HttpRequestMessage req,
        string tenantId,
        string oid,
        ClaimsPrincipal user,
        [DurableClient] IDurableClient durableClient,
        ILogger log)
        {
            try
            {


                if (req.Method == HttpMethod.Get)
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

        [FunctionName("SetApplianceSessionContext")]
        public async Task<ApplianceSessionContextEntity> SetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                                                    [FromBody] ApplianceSessionContextEntity applianceContext)
        {
            return await Task.FromResult(new ApplianceSessionContextEntity());
        }

        //[FunctionName("GetRetentionPolicyForStorageAccount")]
        //public async Task<TableStorageRetentionPolicy> GetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
        //                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        //{
        //    return await Task.FromResult(new TableStorageRetentionPolicy());
        //}

        [FunctionName("SetRetentionPolicyForStorageAccount")]
        public async Task<TableStorageRetentionPolicy> SetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [FunctionName("GetEntityRetentionPolicyForStorageAccount")]
        public async Task<TableStorageEntityRetentionPolicy> GetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                     [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [FunctionName("SetEntityRetentionPolicyForStorageAccount")]
        public async Task<TableStorageEntityRetentionPolicyEntity> SetEntityRetentionPolicyForStorageAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RetentionEntities/SetEntityRetentionPolicyForStorageAccount"
                                                                     + ControlChannelConstants.SetEntityRetentionPolicyForStorageAccountRouteTemplate)]
             HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            ClaimsPrincipal claimsPrincipal,
                    string tenantId, string oid, string policyEntityId, string surfaceEntityId)
        {
     TableStorageEntityRetentionPolicyEntity ret = await GetResponseForSetEntityReentionPolicy(req, durableClient, tenantId, oid, surfaceEntityId);

            return ret;
        }

          private async Task<TableStorageEntityRetentionPolicyEntity> GetResponseForSetEntityReentionPolicy(HttpRequestMessage req, IDurableClient durableClient, string tenantId, string oid, string surfaceEntityId)
        {
            TableStorageEntityRetentionPolicyEntity ret = new TableStorageEntityRetentionPolicyEntity();
            try
            {
                var commandJson = await req.Content.ReadAsStringAsync();
                var command = await commandJson.FromJSONStringAsync<TableStorageEntityRetentionPolicyEntity>();
                var item = command.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.First();
                var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();
                var applianceCtx = (await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient)).EntityState;
                
                applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First()
                    .TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.Where(w => w.Id == item.Id).First().RetentionPeriodInDays = item.RetentionPeriodInDays;

                applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First()
                    .TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.PolicyEnforcementMode = command.PolicyEnforcementMode;

                var newRetentionPeriod = applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First()
                    .TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.Where(w => w.Id == item.Id).First().RetentionPeriodInDays;

                var updated = await this.TableRetentionApplianceEngine.SetCurrentJobOutput(tenantId, oid, applianceCtx, durableClient);
                applianceCtx = (await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient)).EntityState;

                var newerRetentionPeriod = applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First()
                    .TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.Where(w => w.Id == item.Id).First().RetentionPeriodInDays;

                ret = applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First().TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy;
            }
            catch (Exception e)
            {
                int i = 0;
            }

            return ret;
        }

        [FunctionName("SetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicyEntity> SetTableRetentionPolicyForStorageAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "RetentionEntities/SetTableRetentionPolicyForStorageAccount"
                                                                     + ControlChannelConstants.SetTableRetentionPolicyForStorageAccountRouteTemplate)]
             HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            ClaimsPrincipal claimsPrincipal,
            string tenantId, string oid, string policyEntityId, string surfaceEntityId)
        {

            TableStorageTableRetentionPolicyEntity ret = new TableStorageTableRetentionPolicyEntity();
            try
            {
                var commandJson = await req.Content.ReadAsStringAsync();
                var command = await commandJson.FromJSONStringAsync<TableStorageTableRetentionPolicyEntity>();
                var item = command.MetricRetentionSurface.MetricsRetentionSurfaceItemEntities.First();
                var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();
                var applianceCtx = (await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient)).EntityState;
                var tuple = applianceCtx.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Equals(storageAccountId)).First();

                var policy = tuple.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy;
                policy.policyEnforcementMode = command.policyEnforcementMode;

                if (policy.MetricRetentionSurface.Id.Equals(surfaceEntityId))
                {
                    var update = policy.MetricRetentionSurface.MetricsRetentionSurfaceItemEntities.Where(w => w.Id == item.Id).First();
                    update.RetentionPeriodInDays = item.RetentionPeriodInDays;

                    var updated = await this.TableRetentionApplianceEngine.SetCurrentJobOutput(tenantId, oid, applianceCtx, durableClient);
                }

                ret = policy;
            }
            catch (Exception e)
            {
                int i = 0;
            }

            return ret;
        }

        [FunctionName("GetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> GetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
             [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {

            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }


        [FunctionName("GetMetricsRetentionPolicyEnforcementResult")]
        public async Task<HttpResponseMessage> GetMetricsRetentionPolicyEnforcementResult([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetMetricsRetentionPolicyEnforcementResult"
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
             HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
            ClaimsPrincipal claimsPrincipal)
        {
            var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();

            var currentState = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);
            var res = currentState.EntityState.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Contains(storageAccountId)).FirstOrDefault();

            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(await res.TableStoragePolicyEnforcementResult.ToJSONStringAsync());
            return resp;
        }

        [FunctionName("GetDiagnosticsRetentionPolicyEnforcementResult")]
        public async Task<HttpResponseMessage> GetDiagnosticsRetentionPolicyEnforcementResult(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetDiagnosticsRetentionPolicyEnforcementResult"
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
            HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid)
        {
            var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();

            var currentState = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);
            var res = currentState.EntityState.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Contains(storageAccountId)).FirstOrDefault();

            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(await res.TableStorageEntityPolicyEnforcementResult.ToJSONStringAsync());
            return resp;
        }

        [FunctionName("GetRetentionPolicyForStorageAccount")]
        public async Task<HttpResponseMessage> GetRetentionPolicyForStorageAccount([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetRetentionPolicyForStorageAccount"
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
             HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
            ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();

                var currentState = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);
                var res = currentState.
                            EntityState.
                            CurrentJobOutput.
                            retentionPolicyJobs.
                            Where(w => w.StorageAccount != null && w.StorageAccount.Id != null &&
                            w.StorageAccount.Id.Contains(storageAccountId)).
                            FirstOrDefault()?.
                            TableStorageRetentionPolicy;

                if (res != null)
                {
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Content = new StringContent(await res.ToJSONStringAsync());
                    return resp;
                }
                else
                {
                    HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.NotFound);
                    return resp;
                }
            }
            catch (Exception e)
            {
                HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.NotFound);
                return resp;
            }
        }

        /// <summary>
        /// this function might be dead on arrival we'll see
        /// </summary>
        /// <param name="req"></param>
        /// <param name="durableClient"></param>
        /// <param name="durableEntityClient"></param>
        /// <param name="tenantId"></param>
        /// <param name="oid"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        [FunctionName("GetCurrentJobOutput")]
        [Obsolete]

        public async Task<HttpResponseMessage> GetCurrentJobOutput([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/GetCurrentJobOutput"
                                                                     + ControlChannelConstants.QueryWorkflowCheckpointStatusRouteTemplate)]
             HttpRequestMessage req,
            [DurableClient] IDurableClient durableClient,
            [DurableClient] IDurableEntityClient durableEntityClient,
            string tenantId,
            string oid,
            ClaimsPrincipal claimsPrincipal)
        {
            var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();

            var currentState = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);
            var res = currentState.EntityState.CurrentJobOutput;

            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(await res.ToJSONStringAsync());
            return resp;
        }

        [FunctionName("ControlChannelEndpoint")]
        public async Task<HttpResponseMessage> WorkflowOperator(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "RetentionEntities/" + ControlChannelConstants.ApplicationControlChannelEndpoint
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

        [FunctionName("GetApplianceLogEntries")]
        public async Task<HttpResponseMessage> GetApplianceLogEntries(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RetentionEntities/" + "GetApplianceLogEntries"
                                                                             + ControlChannelConstants.GetApplianceLogEntriesRouteTemplate)] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter,
        Microsoft.Azure.WebJobs.ExecutionContext context,
        [DurableClient] IDurableClient durableClient,
        [DurableClient] IDurableEntityClient durableEntityClient,
        ClaimsPrincipal claimsPrincipal,
        string tenantId,
        string oid,
        int offset, int pageSize, int pageCount,
        ILogger log)
        {

            var ret = new HttpResponseMessage(HttpStatusCode.OK);
            bool isAuthorized = await this.TableRetentionApplianceEngine.ApplyAuthorizationStrategy(req.Headers, claimsPrincipal);
            var impersonate = await this.TableRetentionApplianceEngine.GetImpersonationTokenFromHeaders(req.Headers);

            var entityId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<JobOutputLogEntity>(tenantId, oid);
            var currentState = await durableClient.ReadEntityStateAsync<JobOutputLogEntity>(entityId);
            if (currentState.EntityExists)
            {
                // filter the entities to be returned
                var retContent = new List<JobOutputLogEntry>();

                var logs = await currentState.EntityState.getLogEntries(new LogEntryQuery()
                { pageCount = pageCount, pageSize = pageSize, startoffset = offset });

                retContent.AddRange(logs);

                var returnedEntity = new JobOutputLogEntity()
                {
                    userOid = oid,
                    userTenantId = tenantId,
                    logEntries = retContent,
                    rowCount = currentState.EntityState.logEntries.Count()
                }; //  currentState.EntityState;

                ret.Content = new StringContent(JsonConvert.SerializeObject(returnedEntity), Encoding.UTF8,
                                    "application/json");
            }
            else
            {
                ret.Content = new StringContent(JsonConvert.SerializeObject(new JobOutputLogEntity()), Encoding.UTF8,
                                    "application/json");
            }

            return ret;
        }

    }
}

