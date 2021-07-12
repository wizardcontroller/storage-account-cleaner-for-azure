using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models;
using com.ataxlab.functions.table.retention.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using com.ataxlab.functions.table.retention.entities;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using com.ataxlab.azure.table.retention.models.models;
using System.Linq;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using com.ataxlab.azure.table.retention.models.extensions;
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
        public async Task<ApplianceSessionContext> SetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                                                    [FromBody] ApplianceSessionContext applianceContext)
        {
            return await Task.FromResult(new ApplianceSessionContext());
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
        public async Task<TableStorageEntityRetentionPolicy> SetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageEntityRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [FunctionName("GetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> GetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
             [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        [FunctionName("SetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> SetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageTableRetentionPolicy policy)
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
            var storageAccountId = req.Headers.Where(w => w.Key.Contains(ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)).FirstOrDefault().Value.First();

            var currentState = await this.TableRetentionApplianceEngine.GetApplianceContextForUser(tenantId, oid, durableClient);
            var res = currentState.EntityState.CurrentJobOutput.retentionPolicyJobs.Where(w => w.StorageAccount.Id.Contains(storageAccountId)).FirstOrDefault();

            HttpResponseMessage resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(await res.ToJSONStringAsync());
            return resp;
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
    }
}

