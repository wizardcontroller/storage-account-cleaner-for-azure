using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using com.ataxlab.azure.table.retention.services.azuremanagement;
using com.ataxlab.azure.table.retention.services.dashboardapi;
using com.ataxlab.functions.table.retention.dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace com.ataxlab.functions.table.retention.dashboard.rcl.Controllers
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class ConfigController : Controller
    {
        public ConfigController(ITableRetentionDashboardAPI dashboardClient,
                              TableRetentionApplianceScopes requiredScopes,
                               IAzureManagementAPIClient azureMgmtClient,
                                ILogger<ConfigController> logger)
        {
            ApplianceClient = dashboardClient;
            log = logger;
            TableRetentionApplianceScopes = requiredScopes;
            this.AzureManagementClient = azureMgmtClient;
        }

        public ITableRetentionDashboardAPI ApplianceClient { get; }

        private ILogger<ConfigController> log;

        public TableRetentionApplianceScopes TableRetentionApplianceScopes { get; }
        public IAzureManagementAPIClient AzureManagementClient { get; }

        [HttpGet(Name = "GetOperatorPageModel")]
        [Produces("application/json")]
        public async Task<OperatorPageModel> GetOperatorPageModel(string param, string anything)
        {
            var model = await InitializeOperatorPageModel();
            return model;
        }

        private async Task<OperatorPageModel> InitializeOperatorPageModel()
        {
            OperatorPageModel OperatorPageModel = new OperatorPageModel();


            if (this.User.Identity.IsAuthenticated)
            {

                log.LogInformation("building operator page model");

                OperatorPageModel = await this.ApplianceClient.GetOperatorPageModel();

                // ensure subscriptions
                OperatorPageModel.Subscriptions = await this.AzureManagementClient.GetSubscriptionsForLoggedInUser();



                OperatorPageModel.ApplianceSessionContext.AvailableSubscriptions = OperatorPageModel.Subscriptions;

                log.LogInformation("built operator page model with easyauthtoken = " + OperatorPageModel.EasyAuthAccessToken);
                // OperatorPageModel.EasyAuthAccessToken = zumoToken;
                var newclaims = from claim in User.Claims
                                select new
                                {
                                    Type = claim.Type,
                                    Value = claim.Value,
                                    // Issuer = claim.Issuer,
                                    // OriginalIssuer = claim.OriginalIssuer,
                                    // Subject = claim.Subject,
                                    // ValueType = claim.ValueType
                                };

                var claims = JsonConvert.SerializeObject(newclaims.ToList());
                ViewData["Claims"] = claims;
            }

            ViewData["OperatorPageModel"] = OperatorPageModel;
            return OperatorPageModel;
        }

    }
}
