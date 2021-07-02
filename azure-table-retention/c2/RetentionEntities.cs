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

        [FunctionName("EntityChannelFunctions")]
        //[OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }



        [HttpGet(Name = "GetWorkflowCheckpoint")]
        public async Task<WorkflowCheckpointDTO> GetWorkflowCheckpoint([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId)
        {
            return await Task.FromResult(new WorkflowCheckpointDTO());
        }

        [HttpGet(Name = "GetApplianceSessionContext")]
        public async Task<ApplianceSessionContext> GetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId)
        {
            return await Task.FromResult(new ApplianceSessionContext());
        }

        [HttpPost(Name = "SetApplianceSessionContext")]
        public async Task<ApplianceSessionContext> SetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                                                    [FromBody] ApplianceSessionContext applianceContext)
        {
            return await Task.FromResult(new ApplianceSessionContext());
        }

        [HttpGet(Name = "GetRetentionPolicyForStorageAccount")]
        public async Task<TableStorageRetentionPolicy> GetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [HttpPost(Name = "SetRetentionPolicyForStorageAccount")]
        public async Task<TableStorageRetentionPolicy> SetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [HttpGet(Name = "GetEntityRetentionPolicyForStorageAccount")]
        public async Task<TableStorageEntityRetentionPolicy> GetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                     [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [HttpPost(Name = "SetEntityRetentionPolicyForStorageAccount")]
        public async Task<TableStorageEntityRetentionPolicy> SetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageEntityRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [HttpGet(Name = "GetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> GetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
             [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        [HttpPost(Name = "SetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> SetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageTableRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        [HttpPost(Name = "SetOperatorPageModel")]
        public async Task<OperatorPageModel> SetOperatorPageModel([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] OperatorPageModel policy)
        {
            return await Task.FromResult(new OperatorPageModel());
        }
    }
}

