using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.services.dashboardapi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    //[AuthorizeForScopes(Scopes = new[] { "User.Read" })]
    [Authorize]
    [EnableCors("CorsPolicy")]
    public class DashboardApiController : ControllerBase
    {
        private readonly ILogger<DashboardApiController> _logger;

        public TableRetentionApplianceScopes TableRetentionApplianceScopes { get; private set; }

        ITableRetentionDashboardAPI ApplianceClient;

        public DashboardApiController(ITableRetentionDashboardAPI dashboardClient,
                              TableRetentionApplianceScopes requiredScopes,
                                ILogger<DashboardApiController> logger)
        {
            ApplianceClient = dashboardClient;
            _logger = logger;
            TableRetentionApplianceScopes = requiredScopes;
        }

        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "User.Read" })]
        [EnableCors("CorsPolicy")]
        [Authorize]
        public async Task<string> GetApplianceStatus()
        {
            var result = await ApplianceClient.GetCurrentWorkflowCheckpoint();
            
            return JsonConvert.SerializeObject(result);
        }
    }
}
