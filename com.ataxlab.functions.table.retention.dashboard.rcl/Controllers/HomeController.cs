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
using Microsoft.Azure.Management.ResourceManager.Fluent.Core.DAG;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.WebRequestMethods;

namespace com.ataxlab.functions.table.retention.dashboard.Controllers
{

    [AuthorizeForScopes(Scopes = new[] { "https://management.azure.com/user_impersonation", "user.read" })]
    [Route("[controller]/[action]/{oid?}/{subscriptionId?}/{tenantid?}/{workflowoperation?}/{commandId?}")]
    [EnableCors("CorsPolicy")]
    [Authorize]
    public class HomeController : Controller
    {

        public IConfiguration Configuration { get; }
        private const string VIEWBAGKEY_ORCHESTRATION_STATUS = "OrchestrationStatus";
        private readonly ILogger<HomeController> log;

        public TableRetentionApplianceScopes TableRetentionApplianceScopes { get; private set; }

        public ITableRetentionDashboardAPI ApplianceClient { get; set; }
        public IAzureManagementAPIClient AzureManagementClient { get; set; }
        public ITokenAcquisition TokenAcquisition { get; }

        public HomeController(ITableRetentionDashboardAPI dashboardClient, ITokenAcquisition tokenAcquisition,
                              IAzureManagementAPIClient azureMgmtClient, IConfiguration config,
                              TableRetentionApplianceScopes requiredScopes,
                                ILogger<HomeController> logger)
        {
            ApplianceClient = dashboardClient;
            log = logger;
            Configuration = config;
            this.AzureManagementClient = azureMgmtClient;
            this.TokenAcquisition = tokenAcquisition;
            TableRetentionApplianceScopes = requiredScopes;
        }

        [HttpGet(Name = "adminconsent")]
        [Route("/adminconsent")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public async Task<IActionResult> AdminConsent(string tenant, string state, bool admin_consent)
        {
            int i = 0;
            var token = this.TokenAcquisition.GetAccessTokenForUserAsync(new[] { "https://management.azure.com/user_impersonation"});
            string promptConsentUrl = GetDefaultScopeAdminConsentUrl(); // GetAzureManagementPromptConsentUrl();
            return Redirect(promptConsentUrl);

        }

        [HttpGet(Name = "Index")]
        [Route("/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        // [Route("/[controller]")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {



            //if (User.Identity.IsAuthenticated)
            //{
            //    try
            //    {
            //        // ensure subscriptions
            //        var subscriptions = await this.AzureManagementClient.GetSubscriptionsForLoggedInUser();
            //    }
            //    catch (Exception e)
            //    {
            //        var clientId = Configuration["AzureAd:clientId"];
            //        var tenantId = this.AzureManagementClient.GetTenantId();

            //        var consentUrl = $"https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={clientId}";
            //        return Redirect(consentUrl);
            //    }

            //}

            OperatorPageModel OperatorPageModel = new OperatorPageModel();


            try
            {
                OperatorPageModel = await InitializeOperatorPageModel();

                if (User.Identity.IsAuthenticated)
                {

                    var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
                    var idToken = await HttpContext.GetTokenAsync("id_token");
                    log.LogInformation($"id token ={idToken}");
                    log.LogInformation($"refreshToken token ={refreshToken}");


                    if (OperatorPageModel.Subscriptions == null || OperatorPageModel.Subscriptions.Count() == 0)
                    {
                        // if you got here the user is authorized to get subscriptions
                        // and found none
                        var token = this.TokenAcquisition.GetAccessTokenForUserAsync(new string[] { "https://management.azure.com/user_impersonation" });
                        string promptConsentUrl = this.GetManagementAuthorizeUrl();
                        return Redirect(promptConsentUrl);
                    }

                    //var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
                    //if (expTime.DateTime < DateTime.UtcNow)
                    //{
                    //    return Redirect("/MicrosoftIdentity/Account/SignIn");
                    //}
                }

                log.LogInformation("home controller has easy auth token = " + OperatorPageModel.EasyAuthAccessToken);
            }
            catch(MicrosoftIdentityWebChallengeUserException e)
            {
                if (e.InnerException.Message.Contains("AADSTS65001"))
                {
                    // show consent prompt
                    // to deploy service principal for the app
                    // to the target tenant
                    string msalUiUrl = GetAdminConsentPrompt();
                    return Redirect(msalUiUrl);
                }
            }
            catch (Exception e)
            {
                string promptConsentUrl = GetManagementAuthorizeUrl();
                return Redirect(promptConsentUrl);
            }

            return View(OperatorPageModel);
        }


        [HttpGet(Name = "azuremgmtauth")]
        [Route("/azuremgmtauth")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public async Task<IActionResult> AzureManagementAuth(string code)
        {
            int i = 0;
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject("");
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var url = this.GetManagementCodeRedemptionUrl(code);
                var response = await client.PostAsync(url, data);
            }

            return View("Index");

        }

        private string GetManagementCodeRedemptionUrl(string code)
        {
            var url = string.Empty;
            var clientId  = "x";
            var clientSecret = HttpUtility.UrlEncode("");
            var scope = HttpUtility.UrlEncode("https://management.azure.com/user_impersonation");
            var redirectUri = HttpUtility.UrlEncode("");
            var tenantId = "";

            url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token?client_id={clientId}&scope={scope}&code={code}&redirect_uri={redirectUri}&grant_type=authorization_code" +
                $"& code_verifier=ThisIsntRandomButItNeedsToBe43CharactersLong&client_secret={clientSecret}";
            return url;
        }

        private string GetManagementAuthorizeUrl()
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                var clientId = Configuration["AzureAd:clientId"];
                var tenantId = this.AzureManagementClient.GetTenantId();
                var redirect = HttpUtility.UrlEncode($"https://{this.Request.Host}/azuremgmtauth");
                var scope = HttpUtility.UrlEncode("https://management.azure.com/user_impersonation");
                var state = new Random().Next();
                var sha256 = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(state.ToString())).ToString();
                var urlTemplate = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                    $"client_id={clientId}" +
                    $"&response_type=code" +
                    $"&redirect_uri={redirect}" +
                    $"&scope={scope}" +
                    $"&state={state}&code_challenge={sha256}" +
                    $"&code_challenge_method=S256";
                // as per https://stackoverflow.com/questions/53309253/how-can-i-find-the-admin-consent-url-for-an-azure-ad-app-that-requires-microsoft

                // as per 
                // https://stackoverflow.com/questions/39582510/azure-ad-prompt-user-admin-to-re-consent-after-changing-application-permissions
                var promptConsentUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";

                log.LogError($"no subscriptions: redirecting to consent url {promptConsentUrl}");
                return promptConsentUrl;
            }
        }


        private string GetAdminConsentPrompt()
        {
            var clientId = Configuration["AzureAd:clientId"];
            var tenantId = this.AzureManagementClient.GetTenantId();
            var redirect = $"https://{this.Request.Host}/adminconsent";
            var scope = $"openid+profile";
            var state = new Random().Next();
            // as per https://stackoverflow.com/questions/53309253/how-can-i-find-the-admin-consent-url-for-an-azure-ad-app-that-requires-microsoft
            var consentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/adminconsent?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";
            var notpromptConsentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0?prompt=consent?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";

            // as per 
            // https://stackoverflow.com/questions/39582510/azure-ad-prompt-user-admin-to-re-consent-after-changing-application-permissions
            var promptConsentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/adminconsent?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";

            log.LogError($"no subscriptions: redirecting to consent url {promptConsentUrl}");
            return promptConsentUrl;
        }

        private string GetDefaultScopeAdminConsentUrl()
        {
            var dashboardAppUri = Configuration["Dashboard:AppUri"];
            var clientId = Configuration["AzureAd:clientId"];
            var tenantId = this.AzureManagementClient.GetTenantId();
            var redirect = $"https://{this.Request.Host}";
            var scope = HttpUtility.UrlEncode($"https://management.azure.com/user_impersonation");
            var state = new Random().Next();
            // as per https://stackoverflow.com/questions/53309253/how-can-i-find-the-admin-consent-url-for-an-azure-ad-app-that-requires-microsoft
            var consentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/adminconsent?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";
            var notpromptConsentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0?prompt=consent?client_id={clientId}&redirect_uri={redirect}&scope={scope}&state={state}";

            // as per 
            // https://stackoverflow.com/questions/39582510/azure-ad-prompt-user-admin-to-re-consent-after-changing-application-permissions
            var promptConsentUrl = $"https://login.microsoftonline.com/{tenantId}/v2.0/adminconsent?client_id={clientId}&redirect_uri={redirect}&scope={scope}";

            log.LogError($"no subscriptions: redirecting to consent url {promptConsentUrl}");
            return promptConsentUrl;
        }



        private async Task<OperatorPageModel> InitializeOperatorPageModel()
        {
            OperatorPageModel OperatorPageModel = new OperatorPageModel();



            if (this.User.Identity.IsAuthenticated)
            {

                log.LogInformation("building operator page model");


                OperatorPageModel = await this.ApplianceClient.GetOperatorPageModel();


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

        [HttpGet(Name = "BeginEnvironmentDiscovery")]
        [Route("/Dashboard/BeginEnvironmentDiscovery/{commandId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
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


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet(Name = "GetCofigureAppliance")]
        [Route("/Operator/ConfigureAppliance")]
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

                if (validSubscription == null)
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
            if (subscriptionId == null && OperatorPageModel.ApplianceSessionContext.SelectedSubscriptionId == string.Empty)
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
        [HttpPost(Name = "ConfigureAppliance")]
        [ValidateAntiForgeryToken]
        [ApiExplorerSettings(IgnoreApi = true)]
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

                foreach (var acct in applianceSessionContext.AvailableStorageAccounts
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
                    return View(nameof(HomeController.Index), OperatorPageModel);
                }

            }

            return View(nameof(HomeController.Operator), OperatorPageModel);
        }

        [HttpGet(Name = "SelectSubscription")]
        [Route("/dashboard/{oid}/selectsubscription/{subscriptionId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
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
        [HttpGet(Name = "ProvisionAppliance")]
        [Route("/Dashboard/ProvisionAppliance/{commandId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        [HttpGet(Name = "BeginWorkflow")]
        //[Route("BeginWorkflow/{commandId}")]
        [Route("/Dashboard/BeginWorkflow/{commandId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        [HttpGet(Name = "SelectCommand")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        [HttpGet(Name = "Operator")]
        [Route("/Operator")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Operator()
        {
            OperatorPageModel OperatorPageModel = await InitializeOperatorPageModel();

            //ViewData["EasyAuthAccessToken"] = await this.ApplianceClient.EnsureEasyAuth(); //await ApplianceClient.GetCurrentAccessToken(HttpContext);
            return View(nameof(Operator), OperatorPageModel);

        }

        [HttpGet(Name = "Privacy")]
        [Route("/Privacy")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet(Name = "Error")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
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
