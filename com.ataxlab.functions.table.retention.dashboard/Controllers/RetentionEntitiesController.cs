using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.control;
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
        [HttpGet]
        [ActionName("ApplianceSessionContext")]
        public async Task<ApplianceSessionContext> GetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId)
        {
            return await Task.FromResult(new ApplianceSessionContext());
        }

        [HttpPost]
        [ActionName("ApplianceSessionContext")]
        public async Task<ApplianceSessionContext> SetApplianceSessionContext([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                                                    [FromBody] ApplianceSessionContext applianceContext)
        {
            return await Task.FromResult(new ApplianceSessionContext());
        }

        [HttpGet]
        [ActionName("TableStorageRetentionPolicy")]
        public async Task<TableStorageRetentionPolicy> GetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                            [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [HttpPost]
        [ActionName("TableStorageRetentionPolicy")]
        public async Task<TableStorageRetentionPolicy> SetRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageRetentionPolicy());
        }

        [HttpGet]
        [ActionName("TableStorageEntityRetentionPolicy")]
        public async Task<TableStorageEntityRetentionPolicy> GetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                     [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [HttpPost]
        [ActionName("TableStorageEntityRetentionPolicy")]
        public async Task<TableStorageEntityRetentionPolicy> SetEntityRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageEntityRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageEntityRetentionPolicy());
        }

        [HttpGet]
        [ActionName("TableStorageTableRetentionPolicy")]
        public async Task<TableStorageTableRetentionPolicy> GetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
             [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        [HttpPost]
        [ActionName("TableStorageTableRetentionPolicy")]
        public async Task<TableStorageTableRetentionPolicy> SetTableRetentionPolicyForStorageAccount([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] TableStorageTableRetentionPolicy policy)
        {
            return await Task.FromResult(new TableStorageTableRetentionPolicy());
        }

        [HttpGet]
        [ActionName("OperatorPageModel")]
        public async Task<OperatorPageModel> GetOperatorPageModel([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
     [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId)
        {
            return await Task.FromResult(new OperatorPageModel());
        }

        [HttpPost]
        [ActionName("OperatorPageModel")]
        public async Task<OperatorPageModel> SetOperatorPageModel([FromHeader(Name = ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION)] string subscriptionId,
                    [FromHeader(Name = ControlChannelConstants.HEADER_CURRENT_STORAGE_ACCOUNT)] string storageAccountId,
                     [FromBody] OperatorPageModel policy)
        {
            return await Task.FromResult(new OperatorPageModel());
        }
    }
}
