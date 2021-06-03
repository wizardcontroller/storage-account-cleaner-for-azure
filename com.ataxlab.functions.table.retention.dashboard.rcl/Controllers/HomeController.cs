using Azure.Core;
using Azure.Identity;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using com.ataxlab.azure.table.retention.services.azuremanagement;
using com.ataxlab.azure.table.retention.services.dashboardapi;
using com.ataxlab.functions.table.retention.dashboard.Models;
using DurableTask.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.Controllers
{
    //[Route("Dashboard")]


    // [AuthorizeForScopes(Scopes = new[] {  ControlChannelConstants.STORAGEIMPERSONATION, ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION })]
    [AuthorizeForScopes(Scopes = new[] { "/.default" })] // , ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION })]
    [Route("[controller]/[action]/{oid?}/{subscriptionId?}/{tenantid?}/{workflowoperation?}/{commandId?}")]
    [EnableCors("CorsPolicy")]
    [Authorize]
    public class HomeController : Controller
    {


        private const string VIEWBAGKEY_ORCHESTRATION_STATUS = "OrchestrationStatus";
        private readonly ILogger<HomeController> log;

        // public IAzureManagementAPIClient AzureManagementAPIClient { get; }
        public TableRetentionApplianceScopes TableRetentionApplianceScopes { get; private set; }

        public ITableRetentionDashboardAPI ApplianceClient { get; set; }
        public IAzureManagementAPIClient AzureManagementClient { get; set; }
        public HomeController(ITableRetentionDashboardAPI dashboardClient,
                              IAzureManagementAPIClient azureMgmtClient,
                              TableRetentionApplianceScopes requiredScopes,
                                ILogger<HomeController> logger)
        {
            ApplianceClient = dashboardClient;
            log = logger;
            this.AzureManagementClient = azureMgmtClient;
            TableRetentionApplianceScopes = requiredScopes;
        }

        [HttpGet]
        [Route("/")]
        // [Route("/[controller]")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");



            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            log.LogInformation("home controller has easy auth token = " + OperatorPageModel.EasyAuthAccessToken);

            return View(OperatorPageModel);
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

        [HttpGet]
        [Route("/Dashboard/BeginEnvironmentDiscovery/{commandId}")]

        public async Task<ActionResult> BeginEnvironmentDiscovery(string id)
        {

            try
            {
                var candidateCommand = GetCachedAvailableCommandFromSessionById(id);

                if (candidateCommand == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // the user posted a valid command
                    List<AvailableCommand> result = await this.ApplianceClient.BeginEnvironmentDiscovery(candidateCommand);

                }
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));


        }

        /// <summary>
        /// TODO validate that the requesting user oid on the storage accounts
        /// matches the current logged in oid
        /// </summary>
        /// <param name="context"></param>
        /// <param name="operatorPageModel"></param>
        /// <returns></returns>
        private bool IsValidatePostedApplianceSessionContext(ApplianceSessionContext context, OperatorPageModel operatorPageModel)
        {

            bool isValid = false;
            List<bool> testResults = new List<bool>();
            log.LogInformation("now validating posted appliance session context available");

            if (context == null || operatorPageModel == null)
            {
                log.LogWarning("validation failed: context null? {0}, operator pagemodel null? {1}", context == null, operatorPageModel == null);
                return false;
            }

            try
            {
                // test selected a storagea ccount
                testResults.Add(context.AvailableStorageAccounts.Where(w => w.IsSelected == true).Any());

                var selectedStorageAccounts = context.AvailableStorageAccounts.Where(w => w.IsSelected == true).ToList();
                // validate that selected storage accounts are still available
                foreach (var acct in selectedStorageAccounts)
                {
                    log.LogInformation("validating selected storage accounts are still available");
                    testResults.Add(operatorPageModel.ApplianceSessionContext.AvailableStorageAccounts.Where(w => w.Id.Equals(acct.Id)).Any());
                }

            }
            catch (Exception e)
            {
                log.LogError("problem validating posted appliance session context {0}", e.Message);
                return false;
            }


            isValid = !testResults.Where(w => w == false).Any();
            log.LogInformation("finished validating posted appliance session context available: isValid? {0}", isValid);

            return isValid;
        }

        [HttpGet]
        [Route("/ConfigureAppliance")]
        public async Task<IActionResult> GetCofigureAppliance()
        {
            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            // clear any storage accounts 
            OperatorPageModel.ApplianceSessionContext.AvailableStorageAccounts.Clear();
            OperatorPageModel.ApplianceSessionContext.SelectedStorageAccounts.Clear();
            OperatorPageModel.IsMustRenderApplianceConfig = true;
            // choose a useable subscription id 
            string subscriptionId = await ApplyConfigureApplianceSubscriptionIdStrategy(OperatorPageModel);

            // add the subscription to the list of selected subscriptions
            var isValidSubscription = OperatorPageModel.Subscriptions.Count > 0;
            if (isValidSubscription)
            {
                // upsert this into the list of selected subscriptions
                var validSubscription = OperatorPageModel.Subscriptions
                            .Where(s => s.subscriptionId.Equals(subscriptionId))
                            .FirstOrDefault();

                if(validSubscription == null)
                {
                    // can happen when you delete all orchestrations
                    // since we're here you have subscriptions -choose arbitrarily the first one
                    // state should reset once appliance 'first' context is posted
                    validSubscription = OperatorPageModel.Subscriptions.First();
                }
                
                validSubscription.IsSelected = true;

                OperatorPageModel.ApplianceSessionContext.SelectedSubscription = validSubscription;
                OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId = validSubscription.subscriptionId;
            }

            return View(nameof(HomeController.Index), OperatorPageModel);
        }

        /// <summary>
        /// choose a subscription id from the sources available to the
        /// pagemodel context when the user clicks change subscription
        /// </summary>
        /// <param name="OperatorPageModel"></param>
        /// <returns></returns>
        private async Task<string> ApplyConfigureApplianceSubscriptionIdStrategy(OperatorPageModel OperatorPageModel)
        {
            var subscriptionId = HttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);
            if(subscriptionId == null && OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId == string.Empty)
            {
                subscriptionId = OperatorPageModel.Subscriptions.FirstOrDefault().subscriptionId;
            }

            OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId = subscriptionId != null ?
                                    subscriptionId : OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId;
            subscriptionId = OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId;


            return await Task.FromResult<string>(subscriptionId);
        }

        /// <summary>
        /// submission endpoint for form submission 
        /// during appliance provisioning workflow
        /// </summary>
        /// <param name="applianceSessionContext"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/configureappliance")]
        //[Route("{controller}/{oid}/{action}")]
        // [Route("ConfigureAppliance/{Id}")]
        public async Task<IActionResult> ConfigureAppliance(
                            //[Bind("SelectedSubscriptionId,SelectedStorageAccountId,UserOid")]
                            ApplianceSessionContext applianceSessionContext)
        {
            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            var subscriptionId = HttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);
            OperatorPageModel.ApplianceSessionContext.AvailableStorageAccounts = await
                this.ApplianceClient.GetStorageAccounts(subscriptionId,
            HttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN),
            OperatorPageModel.ApplianceSessionContext.UserOid);

            if (ModelState.IsValid && IsValidatePostedApplianceSessionContext(applianceSessionContext, OperatorPageModel) == true)
            {

                log.LogInformation("valid posted appliance session context. preparing for upload to appliance");

                // this is essential right here
                applianceSessionContext.SelectedStorageAccounts.Clear();
                OperatorPageModel.ApplianceSessionContext.SelectedStorageAccounts.Clear();

                log.LogInformation("configuring selected storage accounts");
                applianceSessionContext.CurrentJobOutput = new ApplianceJobOutput();
                applianceSessionContext.CurrentJobOutput.RetentionPolicyJobs = new List<RetentionPolicyTupleContainer>();
                for (int i = 0; i < applianceSessionContext.AvailableStorageAccounts.Count; i++)
                {
                    applianceSessionContext.CurrentJobOutput.RetentionPolicyJobs.Add(new RetentionPolicyTupleContainer()
                    {
                        Id = Guid.NewGuid(),
                        StorageAccount = applianceSessionContext.AvailableStorageAccounts[i],
                        TableStorageRetentionPolicy = new azure.table.retention.models.TableStorageRetentionPolicy()
                        {
                            Id = Guid.NewGuid(),
                            TableStorageEntityRetentionPolicy = new azure.table.retention.models.TableStorageEntityRetentionPolicy(),
                            TableStorageTableRetentionPolicy = new azure.table.retention.models.TableStorageTableRetentionPolicy()
                        }
                    });

                    applianceSessionContext.AvailableStorageAccounts[i].SubscriptionId = applianceSessionContext.SelectedSubscriptionId;
                }

                foreach(var acct in applianceSessionContext.AvailableStorageAccounts
                        .Where(w => w.IsSelected == true))
                {
                    acct.SubscriptionId = applianceSessionContext.SelectedSubscriptionId;
                }

                var selectedAccounts = applianceSessionContext.AvailableStorageAccounts
                        .Where(w => w.IsSelected == true).ToList();
                // move selected storage accounts from availabed list to selected list
                // only selected accounts are posted to the appliance
                // the dashboard always sees the full list
                applianceSessionContext.SelectedStorageAccounts.AddRange(selectedAccounts);

                // update the operator pagemodel vis a vis selected storage accounts
                // that will be posted to the appliance
                OperatorPageModel.ApplianceSessionContext.SelectedStorageAccounts
                    .AddRange(selectedAccounts);

                // update the context with the selected subscription
                applianceSessionContext.SelectedSubscriptionId = HttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);
                var tenantId = this.ApplianceClient.GetTenantIdFromUserClaims();
                applianceSessionContext.TenantId = tenantId;
                foreach (var acct in selectedAccounts)
                {
                    // TODO make this a guarantee 
                    acct.TenantId = tenantId;
                }

                var oid = this.ApplianceClient.GetUserOidFromUserClaims();
                var configureResult = await this.ApplianceClient.SetApplianceSessionContext(tenantId,
                                      oid,
                                      applianceSessionContext);

                if (IsValidatePostedApplianceSessionContext(configureResult, OperatorPageModel))
                {
                    // update succeded
                    OperatorPageModel.IsMustRenderApplianceConfig = false;
                    OperatorPageModel.ApplianceSessionContext = configureResult;
                }
                else
                {
                    // update failed
                    OperatorPageModel.IsMustRenderApplianceConfig = true;
                    return RedirectToAction(nameof(HomeController.Index), OperatorPageModel);
                }

            }

            return RedirectToAction(nameof(HomeController.Operator), OperatorPageModel);
        }

        [HttpGet]
        [Route("/dashboard/{oid}/selectsubscription/{subscriptionId}")]
        public async Task<ActionResult> SelectSubscription(string oid, string subscriptionId)
        {
            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            try
            {
                if (User.Identity.IsAuthenticated)
                {

                    log.LogInformation("getting storage accounts");

                    OperatorPageModel.ApplianceSessionContext.AvailableStorageAccounts = await
                        this.ApplianceClient.GetStorageAccounts(subscriptionId,
                                HttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN),
                    OperatorPageModel.ApplianceSessionContext.UserOid);


                    var validSubscription = OperatorPageModel.Subscriptions
                    .Where(s => s.subscriptionId.Equals(subscriptionId))
                    .FirstOrDefault();

                    validSubscription.IsSelected = true;

                    OperatorPageModel.ApplianceSessionContext.SelectedSubscription = validSubscription;
                    OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId = validSubscription.subscriptionId;
                    this.HttpContext.Session.SetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION, validSubscription.subscriptionId);
                }
                else
                {
                    return View(nameof(HomeController.Index));
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting storage accounts {0}", e.Message);
            }

            if (oid.Equals(OperatorPageModel.ApplianceSessionContext.UserOid, StringComparison.InvariantCultureIgnoreCase))
            {
                // triage whatever this was TODO
            }

            OperatorPageModel.IsMustRenderApplianceConfig = true;
            return View(nameof(HomeController.Index), OperatorPageModel);
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/Dashboard/ProvisionAppliance/{commandId}")]

        public async Task<ActionResult> ProvisionAppliance(string commandId)
        {

            try
            {
                var candidateCommand = GetCachedAvailableCommandFromSessionById(commandId);

                if (candidateCommand == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var subscriptionId = HttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);
                    Debug.WriteLine("using subscription id {0}", subscriptionId);
                    // the user posted a valid command
                    var result = await this.ApplianceClient.ProvisionAppliance(candidateCommand);
                    int i = 0;
                }
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Operator));
        }

        [HttpGet]
        //[Route("BeginWorkflow/{commandId}")]
        [Route("/Dashboard/BeginWorkflow/{commandId}")]
        public async Task<IActionResult> BeginWorkflow(string commandId)
        {

            try
            {
                var candidateCommand = GetCachedAvailableCommandFromSessionById(commandId);

                if (candidateCommand == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // the user posted a valid command
                    var result = await this.ApplianceClient.BeginWorkflow(candidateCommand);
                    int i = 0;
                }
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Operator));
        }

        [HttpGet]


        //[Route("/dashboard/dispatch/{tenantid}/{workflowoperation}/{commandid}")]
        public async Task<IActionResult> SelectCommand(
            string oid, string subscriptionId,
            string tenantid,
            string workflowoperation,
            string commandId
            )
        {

            try
            {
                var candidateCommand = GetCachedAvailableCommandFromSessionById(commandId);

                if (candidateCommand == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // the user posted a valid command
                    var result = await this.ApplianceClient.EnsureWorkflowOperation(candidateCommand);
                    int i = 0;
                }
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Operator));
        }

        [HttpGet]
        [Route("/Operator")]
        public async Task<IActionResult> Operator()
        {
            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            //ViewData["EasyAuthAccessToken"] = await this.ApplianceClient.EnsureEasyAuth(); //await ApplianceClient.GetCurrentAccessToken(HttpContext);
            return View(nameof(Operator), OperatorPageModel);

        }

        [HttpGet]
        [Route("/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private AvailableCommand GetCachedAvailableCommandFromSessionById(string id)
        {
            AvailableCommand ret;
            List<AvailableCommand> commands = GetCachedAvailableCommandsFromSession();
            ret = commands?.Where(w => w.AvailableCommandId.Equals(id)).First();
            return ret;
        }


        private List<AvailableCommand> GetCachedAvailableCommandsFromSession()
        {
            // get the available commands from the session
            return JsonConvert.
                                DeserializeObject<List<AvailableCommand>>
                                    (this.HttpContext.Session.GetString(DashboardConstants.SESSIONKEY_AVAILABLECOMMANDS));
        }

    }
}
