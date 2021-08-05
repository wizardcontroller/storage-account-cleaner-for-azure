using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.Controllers
{
    [Route("api/[controller]/[Action]/{tenantId}/{oid}")]
    [Produces("application/json")]
    [ApiController]
    public class RetentionEntitiesController : ControllerBase
    {

        [HttpGet(Name = "GetWorkflowCheckpoint")]
        public async Task<WorkflowCheckpointDTO> GetWorkflowCheckpoint(
            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId)
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

        /// <summary>
        /// set a retentoin policy for a specific table
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="storageAccountId"></param>
        /// <param name="tableStorageTableRetentionPolicyEntityId"></param>
        /// <param name="metricRetentionSurfaceEntityId"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        [HttpPost("{tableStorageTableRetentionPolicyEntityId},{metricRetentionSurfaceEntityId}", Name = "SetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> SetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                    string tenantId, string oid, string tableStorageTableRetentionPolicyEntityId, string metricRetentionSurfaceEntityId,
                     [FromBody] MetricsRetentionSurfaceItemEntity policy)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        /// <summary>
        /// set a retention policy or a specific table
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="storageAccountId"></param>
        /// <param name="tableStorageEntityRetentionPolicyEntityId"></param>
        /// <param name="diagnosticsRetentionSurfaceEntityId"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        [HttpPost("{tableStorageEntityRetentionPolicyEntityId},{diagnosticsRetentionSurfaceEntityId}", Name = "SetEntityRetentionPolicyForStorageAccount")]
        public async Task<TableStorageEntityRetentionPolicy> SetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
             string tenantId, string oid, string tableStorageEntityRetentionPolicyEntityId, string diagnosticsRetentionSurfaceEntityId,
             [FromBody] DiagnosticsRetentionSurfaceItemEntity policy)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
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
                     [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId, 
                     string tableStorageEntityRetentionPolicyEntityId, string diagnosticsRetentionSurfaceEntityId)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        

        [HttpGet(Name = "GetTableRetentionPolicyForStorageAccount")]
        public async Task<TableStorageTableRetentionPolicy> GetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
             [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId, string tableStorageTableRetentionPolicyEntityId, string metricRetentionSurfaceEntity)
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

        [HttpGet(Name = "GetMetricsRetentionPolicyEnforcementResult")]
        public async Task<TableStorageEntityRetentionPolicyEnforcementResult> GetMetricsRetentionPolicyEnforcementResult([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicyEnforcementResult());
        }

        [HttpGet(Name = "GetDiagnosticsRetentionPolicyEnforcementResult")]
        public async Task<TableStorageTableRetentionPolicyEnforcementResult> GetDiagnosticsRetentionPolicyEnforcementResult([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicyEnforcementResult());
        }

        //[HttpGet(Name = "GetRetentionPolicyForStorageAccount")]
        //public async Task<TableStorageRetentionPolicy> GetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
        //                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        //{
        //    return await Task.FromResult(new TableStorageRetentionPolicy());
        //}

        [HttpGet(Name = "GetRetentionPolicyForStorageAccount")]
        public async Task<TableStorageRetentionPolicy> GetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
        [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [HttpGet(Name = "GetCurrentJobOutput")]
        public async Task<ApplianceJobOutput> GetCurrentJobOutput([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
        [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new ApplianceJobOutput());
        }

        [HttpPost(Name = "WorkflowOperator")]
        public async Task<WorkflowCheckpointDTO> WorkflowOperator(
        [FromBody] WorkflowOperationCommand command,
        [FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
        [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new WorkflowCheckpointDTO());
        }

        // [Route("{offset}/{pageSize}/{pageCount}")]
        [HttpGet("{offset}/{pageSize}/{pageCount}", Name = "GetApplianceLogEntries")]
        public async Task<JobOutputLogEntity> GetApplianceLogEntries(string tenantId, string oid, int offset, int pageSize, int pageCount)
        {
            return await Task.FromResult(new JobOutputLogEntity());
        }
    }
}
