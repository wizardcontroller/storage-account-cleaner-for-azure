using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity;
using Microsoft.Identity.Web;
using static System.Formats.Asn1.AsnWriter;
using com.ataxlab.azure.table.retention.models.models.auth;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Microsoft.Identity.Client;
using com.ataxlab.azure.table.retention.models.models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Azure.Core;
using Newtonsoft.Json.Linq;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using com.ataxlab.azure.table.retention.services.azuremanagement;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Rest;
using Microsoft.Azure.Management.Storage;
using System.Security.Cryptography;
using System.Security.Policy;

namespace com.ataxlab.azure.table.retention.services.dashboardapi
{
    public class TableRetentionDashboardAPI : ITableRetentionDashboardAPI
    {
        private HttpContext CurrentHttpContext { get; set; }


        private ILogger<TableRetentionDashboardAPI> log;

        public IConfiguration Configuration { get; }
        public IAzureManagementAPIClient AzureManagementAPIClient { get; }
        public TableRetentionApplianceScopes Scopes { get; private set; }
        public HttpClient HttpClient { get; set; }
        public string CurrentEasyAuthToken { get; private set; }

        readonly ITokenAcquisition TokenAcquisitionHelper;

        /// <summary>
        /// so cool they inject the auth token we need
        /// to call the appliance and ew meet cool with cool
        /// and inject the scopes the api will need
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="tokenAcquisition"></param>
        /// <param name="scopes"></param>
        public TableRetentionDashboardAPI(HttpClient httpClient, IHttpContextAccessor ctx, ITokenAcquisition tokenAcquisition,
                                            IAzureManagementAPIClient azureMgmtClient,
            TableRetentionApplianceScopes scopes, IConfiguration config, ILogger<TableRetentionDashboardAPI> log)
        {
            this.log = log;
            this.Configuration = config;
            this.AzureManagementAPIClient = azureMgmtClient;
            this.CurrentHttpContext = ctx.HttpContext;

            // right now there are men out there puffing their chests
            // to their women and extolling the virtues of their
            // httpclient configuration techniques
            // so we can't fuck this up here
            this.Scopes = scopes;

            // obviously we injected it with the configured base uri
            HttpClient = httpClient;
            HttpClient.Timeout = new TimeSpan(0, 0, 30);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept
                .Add(GetMediaTypeJson());

            this.TokenAcquisitionHelper = tokenAcquisition;
        }

        public async Task<List<StorageAccountDTO>> GetStorageAccounts(string subscriptionid, string impersonationToken, string requestingOid)
        {
            List<StorageAccountDTO> ret = new List<StorageAccountDTO>();
            try
            {
                // userToken = req.Headers.Where(h => h.Key.Equals("USERTOKEN")).Select(s => s.Value)?.FirstOrDefault();
                log.LogInformation("impersonation token = {0}", impersonationToken);

                log.LogInformation("testing USERTOKEN with subscription {0}", subscriptionid);

                StorageManagementClient storage = new StorageManagementClient(new TokenCredentials(impersonationToken));
                storage.SubscriptionId = subscriptionid;

                log.LogInformation("testing USERTOKEN StorageManagementClient for subscription {0}", storage?.SubscriptionId);

                // TODO implement the continuation pattern here
                var storageAccounts = await storage?.StorageAccounts?.ListAsync();
                if (storageAccounts != null)
                {
                    // filter out BlobStorage 
                    foreach (var act in storageAccounts.Where(w => w.Kind.ToLower().Contains("storagev2")))
                    {
                        ret.Add(new StorageAccountDTO()
                        {
                            Id = act.Id,
                            Location = act.Location,
                            Name = act.Name,
                            SkuName = act.Sku.Name,
                            StorageAccountKind = act.Kind,
                            StorageAccountType = act.Type,
                            RequestingAzureAdUserOid = requestingOid,
                            SubscriptionId = subscriptionid,
                            TenantId = GetTenantIdFromUserClaims()
                        });
                    }
                }

                log.LogInformation("storage accounts found {0}", storageAccounts?.Count());

            }
            catch (Exception e)
            {
                /*
				 * 
				 * The access token has been obtained for wrong audience or resource
				 * 'appid'. 
				 * It should exactly match with one of the allowed audiences 
				 * 'https://management.core.windows.net/','https://management.core.windows.net',
				 * 'https://management.azure.com/','https://management.azure.com'.
				 **/
                log.LogError("storage experiment failed due to {0}", e.Message);
            }

            return ret;
        }

        /// <summary>
        /// throws MSAL UI related exceptions
        /// </summary>
        /// <returns></returns>
        public async Task<OperatorPageModel> GetOperatorPageModel()
        {
            OperatorPageModel operatorPageModel = new OperatorPageModel();
            string oid = string.Empty;
            string tenantid = string.Empty;

            // populate the client side urls 
            if (this.CurrentHttpContext.User.Identity.IsAuthenticated)
            {
                try
                {
                    try
                    {
                        operatorPageModel = await EnsureAuthScopesforOperatorPageModel();
                    }
                    catch(Exception e)
                    {
                        log.LogError($"problem ensuring access tokens for page model");
                        throw;
                    }

                    oid = GetUserOidFromUserClaims();
                    tenantid = GetTenantIdFromUserClaims();

                    // may be null at this point
                    operatorPageModel.SelectedSubscriptionId = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);

                    operatorPageModel.ImpersonationToken = this.CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN);
                    operatorPageModel.Oid = oid;
                    operatorPageModel.Tenantid = tenantid;

                    var routeParam = new ControlChannelRouteParameter()
                    {
                        EndPoint = ControlChannelConstants.QueryWorkflowCheckpointStatusEndpoint,
                        RouteFormatTemplate = ControlChannelConstants.QueryWorkflowCheckpointStatusRouteFormatTemplate
                    };

                    var queryWorkflowStatusTemplateRoute = await this.GetTemplateUrlForRoute(routeParam, oid, tenantid);

                    log.LogTrace("populationg operator pagemodel with workflow checkpoint status");
                    operatorPageModel.QueryWorkflowCheckpointStatusEndpoint = queryWorkflowStatusTemplateRoute;


                    var resetStateParams = new ControlChannelRouteParameter()
                    {
                        EndPoint = ControlChannelConstants.DeleteWorkflowCheckpointEditModeEndPoint,
                        RouteFormatTemplate = ControlChannelConstants.DeleteWorkflowCheckpointEditModeRouteFormatTemplate
                    };

                    var resetRoute = await this.GetTemplateUrlForRoute(resetStateParams, oid, tenantid);
                    operatorPageModel.ResetDeviceUrl = resetRoute;
                    //OperatorPageModel.QueryWorkflowCheckpointStatusEndpoint = await this.GetTemplateUrlForRoute(ControlChannelConstants.QueryWorkflowCheckpointStatusEndpoint);
                }
                catch (Exception e)
                {
                    log.LogError("error populationg operator pagemodel with workflow checkpoint status {0}", e.Message);
                    throw;
                }

            }
            /// this model is only useful if the user is authenticated
            operatorPageModel.ApplianceUrl.Add(this.HttpClient.BaseAddress.AbsoluteUri);

            // detect unprovisioned device
            try
            {
                try
                {
                    log.LogTrace("getting subscriptions for logged in user");
                    var token = await this.TokenAcquisitionHelper.GetAccessTokenForUserAsync(new string[] { "https://management.azure.com/user_impersonation"}, tenantId: this.GetTenantIdFromUserClaims(), user: this.CurrentHttpContext.User);

                    var subscriptionsResult = await this.AzureManagementAPIClient.GetSubscriptionsForLoggedInUser(token);
                    operatorPageModel.Subscriptions = subscriptionsResult;

                    log.LogTrace("got subscriptions for logged in user");
                }
                catch (Exception e)
                {
                    log.LogError("error populating available subscriptions {0}", e.Message);
                    // throw;
                }

                log.LogInformation("getting workflow checkpoint for user");

                var isValidCheckpoint = await this.GetCurrentWorkflowCheckpoint();
                if (isValidCheckpoint == null)
                {

                    operatorPageModel.IsMustRenderApplianceConfig = true;
                    operatorPageModel.ApplianceSessionContext = new ApplianceSessionContext();
                    operatorPageModel.ApplianceSessionContext.UserOid = this.GetUserOidFromUserClaims();
                    operatorPageModel.ApplianceSessionContext.AvailableSubscriptions = operatorPageModel.Subscriptions;
                    operatorPageModel.Tenantid = this.GetTenantIdFromUserClaims();
                    operatorPageModel.Oid = this.GetUserOidFromUserClaims();

                    log.LogWarning("null checkpoint. rendering config wizard");
                    return operatorPageModel;
                }
                else if (isValidCheckpoint.CurrentCheckpoint == WorkflowCheckpointIdentifier.UnProvisioned)
                {
                    log.LogWarning("unprovisioned device checkpoint. rendering config wizard");
                    operatorPageModel.IsMustRenderApplianceConfig = true;
                    operatorPageModel.ApplianceSessionContext = new ApplianceSessionContext();
                    operatorPageModel.ApplianceSessionContext.UserOid = this.GetUserOidFromUserClaims();
                    operatorPageModel.ApplianceSessionContext.AvailableSubscriptions = operatorPageModel.Subscriptions;
                    log.LogWarning("null checkpoint. rendering config wizard");
                    operatorPageModel.Tenantid = this.GetTenantIdFromUserClaims();
                    operatorPageModel.Oid = this.GetUserOidFromUserClaims();

                    return operatorPageModel;

                }

            }
            catch (Exception e)
            {
                operatorPageModel.IsMustRenderApplianceConfig = true;

            }

            if (this.CurrentHttpContext.User.Identity.IsAuthenticated)
            {
                var selectedSubscription = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);

                ApplianceSessionContext applianceContext = new ApplianceSessionContext();
                operatorPageModel.ApplianceSessionContext = applianceContext;
                try
                {
                    log.LogTrace("getting appliance context");

                    // maybe the device doesn't have a context. the operator pagemodel must init one
                    var deviceContext = await this.GetApplianceContext(tenantid, oid);
                    if (deviceContext != null)
                    {
                        // recovered appliance context from device
                        applianceContext = deviceContext;
                        log.LogTrace("got appliance context");
                    }
                    else
                    {
                        operatorPageModel.IsMustRenderApplianceConfig = true;
                        operatorPageModel.ApplianceSessionContext = new ApplianceSessionContext();
                        operatorPageModel.ApplianceSessionContext.UserOid = this.GetUserOidFromUserClaims();
                        operatorPageModel.ApplianceSessionContext.AvailableSubscriptions = operatorPageModel.Subscriptions;

                        log.LogWarning("null checkpoint. rendering config wizard");
                        return operatorPageModel;
                    }
                }
                catch (Exception e)
                {
                    log.LogError("problem getting appliance context {0}", e.Message);
                }

                if (
                    applianceContext.SelectedStorageAccounts == null ||
                    applianceContext.SelectedStorageAccounts.Count == 0)
                {
                    // did not find a context on the appliance
                    // ViewData["AvailableSubscriptions"] = subscriptionsResult;
                    operatorPageModel.ApplianceSessionContext.UserOid = oid;
                    operatorPageModel.IsMustRenderApplianceConfig = true;
                }
                else
                {
                    // found a context on the appliance
                    operatorPageModel.ApplianceSessionContext = applianceContext;

                    // maybe don't use the subscription on appliance context
                    // selectedSubscription = this.AzureManagementAPIClient.GetSubscriptions() // operatorPageModel.ApplianceSessionContext.SelectedSubscriptionId;
                    var subscriptions = await this.AzureManagementAPIClient.GetSubscriptionsForLoggedInUser();
                    if(subscriptions == null || subscriptions.Count() == 0)
                    {
                        operatorPageModel.ApplianceSessionContext.UserOid = oid;
                        operatorPageModel.IsMustRenderApplianceConfig = true;
                    }
                    else
                    {
                        // todo - deploy user interface subscription chooser
                        // for use prior to selection of an appliance context
                        selectedSubscription = subscriptions.First().subscriptionId;
                    }

                    CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION, selectedSubscription);
                    operatorPageModel.SubscriptionId = selectedSubscription;
                    operatorPageModel.SelectedSubscriptionId = selectedSubscription;

                    try
                    {
                        var result = await GetCurrentWorkflowCheckpoint();
                        // cache the available commands in session
                        if (result != null)
                        {
                            var timestamp = result.TimeStamp;
                            this.CurrentHttpContext.Session.SetString(DashboardConstants.SESSIONKEY_AVAILABLECOMMANDS, JsonConvert.SerializeObject(result.AvailableCommands));
                            log.LogInformation("applying aailable commands to operator page model");
                            operatorPageModel.AvailableCommands = result.AvailableCommands;
                        }
                        else
                        {
                            log.LogWarning("no workflow checkpoint found");
                            operatorPageModel.IsMustRenderApplianceConfig = true;
                            operatorPageModel.ApplianceSessionContext = new ApplianceSessionContext();
                            operatorPageModel.ApplianceSessionContext.UserOid = this.GetUserOidFromUserClaims();
                            operatorPageModel.ApplianceSessionContext.AvailableSubscriptions = operatorPageModel.Subscriptions;

                            log.LogWarning("null checkpoint. rendering config wizard");
                            return operatorPageModel;
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError("problem biulding available commands for page model {0}", e.Message);
                        operatorPageModel.IsMustRenderApplianceConfig = true;
                    }

                    operatorPageModel.IsMustRenderApplianceConfig = false;
                }

                var impersonationToken = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN);
                if (!String.IsNullOrEmpty(selectedSubscription)
                    && !String.IsNullOrEmpty(impersonationToken)
                    && !String.IsNullOrEmpty(operatorPageModel.ApplianceSessionContext.UserOid))
                {
                    log.LogInformation("conditions exist to request storage accounts for the current user");
                    operatorPageModel.ApplianceSessionContext.AvailableStorageAccounts =
                        await this.GetStorageAccounts(subscriptionid: selectedSubscription, impersonationToken: impersonationToken,
                        requestingOid: operatorPageModel.ApplianceSessionContext.UserOid);
                }

            }

            if (this.CurrentHttpContext.User.Identity.IsAuthenticated)
            {
                // var oid = this.GetUserOidFromUserClaims();
                // var tenantId = this.GetTenantIdFromUserClaims();
                var fromDays = "5";

                var orchestrationRouteParams = new ControlChannelRouteParameter()
                {
                    EndPoint = ControlChannelConstants.QueryOrchestrationStatusEndpoint,
                    RouteFormatTemplate = ControlChannelConstants.QueryOrchestrationStatusRouteFormatTemplate
                };

                try
                {
                    log.LogTrace("populating orchestrations");
                    var orchestrationStatus = await this
                        .QueryAppliance<List<DurableOrchestrationStateDTO>>(System.Net.Http.HttpMethod.Get,
                        await this.GetTemplateUrlForRoute(orchestrationRouteParams, oid, tenantid, fromDays));

                    // ViewData[VIEWBAGKEY_ORCHESTRATION_STATUS] = orchestrationStatus;
                    operatorPageModel.Orchestrations = orchestrationStatus;
                    log.LogTrace("done populating orchestrations");

                }
                catch (Exception e)
                {
                    log.LogError("exception populating orchestrations {0}", e.Message);
                }
            }

            return operatorPageModel;
        }

        private async Task<OperatorPageModel> EnsureAuthScopesforOperatorPageModel()
        {
            OperatorPageModel OperatorPageModel = new OperatorPageModel();
            var eventualAccessToken = string.Empty;
            TokenAcquisitionOptions opts = new TokenAcquisitionOptions();
            try
            {

                if (string.IsNullOrEmpty(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN)))
                {

                    log.LogInformation("initializing Session.UserToken");

                    var appUri = Configuration["ApplianceAppUri"]; // => incorrect Configuration["Dashboard:AppUri"]; // 
                    var eachScope = new List<string>()
                    // { appUri + "/user_impersonation",  ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION};
                    { appUri + "/user_impersonation"};

                    var token = await this.CurrentHttpContext.GetTokenAsync("access_token");
 


                    foreach (var s in eachScope)
                    {
                            AuthenticationResult impersonationResult = await TokenAcquisitionHelper

                                                        .GetAuthenticationResultForUserAsync(scopes: new List<string>() { s }, this.GetTenantIdFromUserClaims());
                        token = impersonationResult.AccessToken;

                        // token = await TokenAcquisitionHelper.GetAccessTokenForUserAsync(new[] { s } ,tenantId: this.GetTenantIdFromUserClaims(), user: this.CurrentHttpContext.User);

                        CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_ACCESS_TOKEN, token);
                        CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_IDTOKEN, impersonationResult.IdToken); 
                        OperatorPageModel.ImpersonationToken = token;

                        eventualAccessToken = token;
                        // aud	
                        // iss	https://login.microsoftonline.com/{tenant}/v2.0
                        // scp	Manage.Appliance Storage.Account.List Storage.Account.Read Storage.Table.Delete Storage.Table.Entity.Delete Storage.Table.List user_impersonation

                    }

                }
                else
                {
                    OperatorPageModel.AccessToken = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN);
                }

                OperatorPageModel.AccessToken = eventualAccessToken;
                OperatorPageModel.EasyAuthAccessToken = await this.EnsureEasyAuth();
                Debug.WriteLine("easyauth token is {0}", OperatorPageModel.EasyAuthAccessToken);
                // iss	https://easyauthsite.azurewebsites.net/
                // aud	https://easyauthsite.azurewebsites.net/

            }
            catch (Exception e)
            {
                log.LogError("problem building operator model {0}", e.Message);
                throw;
            }

            return OperatorPageModel;
        }

        public string GetUserOidFromUserClaims()
        {
            log.LogInformation("getting user oid from claims");
            // get the appliance context
            var oid = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_OID)).FirstOrDefault()?.Value;
            return oid;
        }

        public string GetTenantIdFromUserClaims()
        {
            var tenantId = string.Empty;
            var configuredTenantId = Configuration["AzureAd:TenantId"];

            if (configuredTenantId.Contains("organizations"))
            {
                // return tenant id from claims
                // var tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_TENANT_UTID)).FirstOrDefault()?.Value;
                //var stsUrl = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains("identityprovider")).FirstOrDefault()?.Value;
                //log.LogInformation($"sts url = {stsUrl}");
                //var splitStsUrl = stsUrl.Split("https://sts.windows.net/");
                //tenantId = splitStsUrl.LastOrDefault().TrimEnd('/');
                var tenantClaim = "http://schemas.microsoft.com/identity/claims/tenantid";
                log.LogInformation($"getting tenant id from user claims {tenantClaim}");
                tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(tenantClaim)).FirstOrDefault()?.Value;
                if (tenantId == null || tenantId.Equals(string.Empty))
                {
                    log.LogWarning($"tenantid not found in claim {tenantClaim}");
                    tenantClaim = "tid";
                    log.LogInformation($"getting tenant id from user claims {tenantClaim}");
                    tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(tenantClaim)).FirstOrDefault()?.Value;

                }


                // hardcode multitenant tenantid
                // tenantId = "organizations";
            }
            else
            {
                log.LogInformation($"getting tenant id from user claims {ControlChannelConstants.CLAIM_TENANT_UTID}");
                tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_TENANT_UTID)).FirstOrDefault()?.Value;
            }
            return tenantId;
        }

        public async Task<List<AvailableCommand>> BeginEnvironmentDiscovery(AvailableCommand command)
        {
            List<AvailableCommand> ret = await EnsureWorkflowOperation(command);

            return ret;
        }

        public async Task<List<AvailableCommand>> EnsureWorkflowOperation(AvailableCommand command)
        {
            List<AvailableCommand> ret = new List<AvailableCommand>();

            try
            {
                ConfigureHttpClientHeaders();

                WorkflowOperationCommand cmd = new WorkflowOperationCommand()
                {
                    CandidateCommand = command,
                    CommandCode = command.WorkflowOperation,
                    TimeStamp = DateTime.UtcNow
                };

                var tempUri = await this.GetTemplateUrlForRoute(ControlChannelConstants.ApplicationControlChannelEndpoint);

                log.LogTrace("calling beginworkflow with uri {0}", tempUri);
                var request = new HttpRequestMessage(HttpMethod.Post, tempUri);


                var baseuri = HttpClient.BaseAddress.ToString();
                Debug.WriteLine("httpclient url " + request.RequestUri.ToString());

                request.Headers.Accept.Add(GetMediaTypeJson());
                request.Content = new StringContent(JsonConvert.SerializeObject(cmd),
                                    Encoding.UTF8,
                                    "application/json");
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var applianceStatus = JsonConvert.DeserializeObject<WorkflowCheckpointDTO>(content);


            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(this.Scopes.DefaultApplianceScope, ex.MsalUiRequiredException);
                // return string.Empty;
            }
            catch (MsalUiRequiredException ex)
            {
                await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(this.Scopes.DefaultApplianceScope, ex);

                //return string.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception calling api " + e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            return ret;
        }

        public async Task<List<AvailableCommand>> ProvisionAppliance(AvailableCommand command)
        {
            List<AvailableCommand> ret = await EnsureWorkflowOperation(command);



            return ret;
        }

        private MediaTypeWithQualityHeaderValue GetMediaTypeJson()
        {
            return new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTemplateUrlForRoute(ControlChannelRouteParameter routeTemplate, params string[] templateParams)
        {
            var url = string.Empty;
            try
            {
                var urlPath = String.Format(routeTemplate.RouteFormatTemplate, templateParams);
                url = routeTemplate.EndPoint + urlPath;
            }
            catch (Exception e)
            {
                log.LogError("problem getting route template {0}", routeTemplate);
            }

            return await Task.FromResult<string>(url);
        }

        /// <summary>
        /// applies format.string to route template
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTemplateUrlForRoute(string endoint)
        {
            var tenantId = this.GetTenantIdFromUserClaims();
            var oid = this.GetUserOidFromUserClaims();

            var urlPath = String.Format(ControlChannelConstants.ApplianceContextRouteFormatTemplate, tenantId, oid);
            var url = endoint + urlPath;
            return await Task.FromResult<string>(url);
        }

        /// <summary>
        /// acquire an access token for the downstream service
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetCurrentEasyAuthToken()
        {
            log.LogInformation("GetCurrentAccessToken");
            string ret = string.Empty;

            if (string.IsNullOrEmpty(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN)))
            {
                log.LogInformation(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN + " not found in session. getting current access token");
                var tenantId = this.GetTenantIdFromUserClaims(); // Configuration["AzureAd:TenantId"];
                // user.read is good for getting a graph token for the graph api audience, which is this 00000003-0000-0000-c000-000000000000"
                log.LogInformation("tenant id " + tenantId);



                log.LogInformation("getting easyauth token with access token = " + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN));
                var appserviceEasyAuthToken = await GetEasyAuthTokenForCurrentUser();
                CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN, appserviceEasyAuthToken);
                log.LogInformation("got easyauth token = " + appserviceEasyAuthToken);
                this.CurrentEasyAuthToken = appserviceEasyAuthToken;
                ret = appserviceEasyAuthToken;
            }
            else
            {

                log.LogInformation("GetCurrentAccessToken found a cached easyauth token");

                log.LogInformation("got easyauth token from session = " + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN));
                ret = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN);

            }


            return ret; // appserviceEasyAuthToken;
        }

        public async Task<string> EnsureEasyAuth()
        {
            log.LogInformation("ensuring easy auth. caching token in session");

            var result = string.Empty;
            Microsoft.Identity.Client.AuthenticationResult impersonationResult = null;
            var currentToken = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN);
            var tenantId = this.GetTenantIdFromUserClaims();
            if (string.IsNullOrEmpty(currentToken))
            {
                log.LogInformation("initializing Session.UserToken");
                var eachScope = new List<string>()
                    {
                    Configuration["Dashboard:AppUri"] + "/user_impersonation",
                        // Configuration["ApplianceAppUri"] + "/user_impersonation",
                        ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION
                    };
                foreach (var s in eachScope)
                {
                    impersonationResult = await TokenAcquisitionHelper
                    .GetAuthenticationResultForUserAsync(scopes: new List<string>() { s }, tenantId: tenantId);
                }

                CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_ACCESS_TOKEN, impersonationResult.AccessToken);
                CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_IDTOKEN, impersonationResult.IdToken);
                log.LogInformation("setting http client USERTOKEN = " + impersonationResult.AccessToken);
                this.HttpClient.DefaultRequestHeaders.Add(ControlChannelConstants.SESSION_ACCESS_TOKEN, impersonationResult.AccessToken);
            }
            else
            {
                log.LogInformation("http USERTOKEN request header already set =" + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN));
                this.HttpClient.DefaultRequestHeaders.Add(ControlChannelConstants.SESSION_ACCESS_TOKEN, CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN));
            }



            // cache the easy auth access token
            if (string.IsNullOrEmpty(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN)))
            {
                log.LogInformation("calling GetCurrentAccessToken");
                // context.Session.SetString(SESSION_KEY_EASYAUTHTOKEN, await this.GetCurrentAccessToken());
                result = await this.GetCurrentEasyAuthToken();
                CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN, result);
                log.LogInformation("setting http client x-zumo-auth var = " + result);

                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ControlChannelConstants.HEADER_X_ZUMO_AUTH, result);

            }
            else
            {
                var cachedToken = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN);
                log.LogInformation("setting http client x-zumo-auth from cached session var = " + cachedToken);
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ControlChannelConstants.HEADER_X_ZUMO_AUTH, cachedToken);
                result = cachedToken;
            }


            return result;
        }

        public async Task<ApplianceSessionContext> GetApplianceContext(string tenantId, string oid)
        {

            var ret = new ApplianceSessionContext();
            try
            {
                var routeParam = new ControlChannelRouteParameter()
                {
                    EndPoint = ControlChannelConstants.ApplianceContextEndpoint,
                    RouteFormatTemplate = ControlChannelConstants.ApplianceContextRouteFormatTemplate
                };

                var route = await this.GetTemplateUrlForRoute(routeParam, oid, tenantId);

                //var url = await this.GetTemplateUrlForRoute(ControlChannelConstants.ApplianceContextEndpoint);
                log.LogTrace("getting appliance context from uri {0}", route);

                var result = await this.QueryAppliance<ApplianceSessionContext>(HttpMethod.Get, route);
                ret = result;
                log.LogInformation("returning appliance context");
            }
            catch (Exception e)
            {
                log.LogError("problem getting appliance context {0}", e.Message);
            }

            return ret;
        }

        public async Task<ApplianceSessionContext> SetApplianceSessionContext(string tenantid, string oid, ApplianceSessionContext ctx)
        {
            var url = await this.GetTemplateUrlForRoute(ControlChannelConstants.ApplianceContextEndpoint);
            log.LogTrace("getting appliance context from uri {0}", url);

            var json = JsonConvert.SerializeObject(ctx);
            var result = await this.QueryAppliance<ApplianceSessionContext>(HttpMethod.Post, url, json);

            // todo validate that the appliance returned what we posted

            // we more or less expect the result to be what we posted
            return result;
        }

        public async Task<string> GetEasyAuthTokenForCurrentUser()
        {
            var ret = string.Empty;
            log.LogInformation("Calling Appliance Auth Endpoint");
            try
            {

                HttpClient.DefaultRequestHeaders.Clear();

                // TODO clean up the associated magic strings here there and everywhere
                // expect url with trailing /api
                var authUrl = Configuration["EasyAuthBaseUrl"];
                var baseUrl = Configuration["ApplianceBaseUrl"];
                log.LogInformation("appliance configured url " + baseUrl);
                baseUrl = baseUrl.EndsWith("/api/") ? baseUrl.Substring(0, baseUrl.Length - 4) : baseUrl;
                baseUrl = baseUrl.EndsWith("/api") ? baseUrl.Substring(0, baseUrl.Length - 3) : baseUrl;
                log.LogInformation("trimmed appliance url " + baseUrl);

                // if there is a provided easyauth url use that to auth to the back end
                if (!String.IsNullOrEmpty(authUrl))
                {
                    baseUrl = !authUrl.EndsWith("/") ? authUrl + "/" : authUrl;
                }


                var applianceUrl = baseUrl + ".auth/login/aad";
                log.LogInformation("calculated appliance auth url " + applianceUrl);
                var request = new HttpRequestMessage(HttpMethod.Post, applianceUrl);



                var requestBody = @"{" +
                      @" ""id_token"" : """ +
                      CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IDTOKEN) + @"""" +
                      @" ,""access_token"" : """ +
                      CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN) +
                      @"""}";


                log.LogInformation("request body is " + requestBody);
                request.Content = new StringContent(requestBody, Encoding.UTF8,
                    "application/json");

                log.LogInformation("httpclient url " + request.RequestUri.ToString());
                request.Headers.Accept.Add(GetMediaTypeJson());

                // var host = this.HttpClient.BaseAddress.Host;
                // request.Headers.Add("Host", host);
                var response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                log.LogInformation("sending easyauth login request");
                // easy auth returhs json { authenticationToken : "", user : {userId : ""}}
                ret = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<JObject>(ret);

                log.LogInformation("easy auth returned json = " + json);
                var easyAuthToken = json.Value<string>("authenticationToken");
                log.LogInformation("easy auth token is " + easyAuthToken);

                CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN, easyAuthToken);
                //CurrentHttpContext.Session.SetString(ControlChannelConstants.XMSTOKENAADACCESSTOKEN, easyAuthToken);
                return easyAuthToken;
                // var applianceStatus = JsonConvert.DeserializeObject<WorkflowCheckpoint>(content);

                // var ret = applianceStatus;
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                log.LogWarning("MicrosoftIdentityWebChallengeUserException getting easyauth token: consent probably required " + ex.Message);

            }
            catch (MsalUiRequiredException ex)
            {
                log.LogWarning("MsalUiRequiredException getting easyauth token: consent probably required " + ex.Message);

            }
            catch (Exception e)
            {
                log.LogWarning("Exception getting easyauth token: consent probably required " + e.Message);

            }

            return ret;
        }

        /// <summary>
        /// general way of making query requests to the appliance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="endpoint"></param>
        /// <param name="context"></param>
        /// <param name="jsonPayload"></param>
        /// <returns></returns>
        public async Task<T> QueryAppliance<T>(HttpMethod method, string endpoint, string jsonPayload = "") where T : new()
        {
            // todo fix this so you can specify and identify a type without an instance
            var ret = new T();
            try
            {

                ConfigureHttpClientHeaders();
                var request = new HttpRequestMessage(method, endpoint);

                var baseuri = HttpClient.BaseAddress.ToString();

                log.LogInformation("httpclient url " + request.RequestUri.ToString());
                log.LogInformation("calling with ACCESS TOKEN=" + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_ACCESS_TOKEN));
                log.LogInformation("calling with ZUMOTOKEN=" + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN));
                request.Headers.Accept.Add(GetMediaTypeJson());
                if (!String.IsNullOrEmpty(jsonPayload))
                {
                    log.LogInformation("posting content to the endpoint");

                    request.Content = new StringContent(jsonPayload,
                        Encoding.UTF8,
                        "application/json");
                }



                HttpResponseMessage response = null;


                response = await HttpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var applianceResponse = JsonConvert.DeserializeObject<T>(content);

                ret = applianceResponse;
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(this.Scopes.DefaultApplianceScope, ex.MsalUiRequiredException);
                // return string.Empty;
            }
            catch (MsalUiRequiredException ex)
            {
                await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(this.Scopes.DefaultApplianceScope, ex);
                //return string.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception calling api " + e.Message);
                Debug.WriteLine(e.StackTrace);
            }

            return ret;
        }

        /// <summary>
        /// query the appliance for its status
        /// 
        /// called by api controller 
        /// exect
        /// </summary>
        /// <returns></returns>
        public async Task<WorkflowCheckpointDTO> GetCurrentWorkflowCheckpoint()
        {
            WorkflowCheckpointDTO ret = new WorkflowCheckpointDTO();
            string oid = GetUserOidFromUserClaims();
            string tenantid = GetTenantIdFromUserClaims();
            log.LogInformation("getting workflow checkpoint result");
            try
            {
                var routeParam = new ControlChannelRouteParameter()
                {
                    EndPoint = ControlChannelConstants.QueryWorkflowCheckpointStatusEndpoint,
                    RouteFormatTemplate = ControlChannelConstants.QueryWorkflowCheckpointStatusRouteFormatTemplate
                };

                var queryWorkflowStatusTemplateRoute = await this.GetTemplateUrlForRoute(routeParam, oid, tenantid);
                ret = await this.QueryAppliance<WorkflowCheckpointDTO>(HttpMethod.Get, queryWorkflowStatusTemplateRoute); // new WorkflowCheckpointDTO();

            }
            catch (Exception e)
            {
                log.LogError("problem getting current workflow checkpoint {0}", e.Message);
            }

            log.LogInformation("returning workflow checkpoint result");
            return ret;
        }


        private void ConfigureHttpClientHeaders()
        {
            var currentSubscription = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_SELECTED_SUBSCRIPTION);
            var impersonationToken = this.CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN);
            var token = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN);
            log.LogInformation("configuring dashboard rest client for call to appliance");
            // var accessToken = await this.GetCurrentAccessToken();
            log.LogInformation("got current access token X-ZUMO-AUTH =  " + CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_KEY_EASYAUTHTOKEN));
            HttpClient.DefaultRequestHeaders.Clear();
            // inject the azure ad app service easyauth token
            // as per https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to#validate-tokens-from-providers
            HttpClient.DefaultRequestHeaders.Add(ControlChannelConstants.HEADER_X_ZUMO_AUTH, new List<string>() { token });
            HttpClient.DefaultRequestHeaders.Add(ControlChannelConstants.HEADER_CURRENTSUBSCRIPTION, currentSubscription);
            HttpClient.DefaultRequestHeaders.Add(ControlChannelConstants.HEADER_IMPERSONATION_TOKEN, impersonationToken);

        }

        /// <summary>
        /// enforce synchronous discoveries
        /// therefore use the same ID every time for starting discoveries
        /// </summary>
        /// <param name="discoveryInstanceId"></param>
        /// <returns></returns>
        public EnvironmentDiscoveryResult StartEnvironmentDiscovery(string discoveryInstanceId = ControlChannelConstants.DefaultApplianceWorkflowInstanceId,
                                                                    string startDiscoveryEndpoint = ControlChannelConstants.ApplicationControlChannelEndpoint)
        {
            return null;
        }

        public EnvironmentDiscoveryResult GetEnvironmentDiscoveryResult(string discoveryInstanceId = ControlChannelConstants.DefaultApplianceWorkflowInstanceId)
        {
            return null;
        }

        /// <summary>
        /// apply the Storage Table retention policy 
        /// computed during environment discovery by the appliance
        /// </summary>
        /// <returns></returns>
        public TableStorageTableRetentionPolicyEnforcementResult ApplyComputedStorageTableRetentionPolicy()
        {
            return null;
        }

        public TableStorageTableRetentionPolicyEnforcementResult GetApplyComputedStorageTableRetentionPolicyResult()
        {
            return null;
        }

        public TableStorageEntityRetentionPolicyEnforcementResult ApplyComputedStorageEntityRetentionPolicy()
        {
            return null;
        }

        public TableStorageEntityRetentionPolicyEnforcementResult GetApplyComputedStorageEntityRetentionPolicyResult()
        {
            return null;
        }

        public async Task<List<AvailableCommand>> BeginWorkflow(AvailableCommand candidateCommand)
        {
            List<AvailableCommand> ret = await EnsureWorkflowOperation(candidateCommand);
            return ret;
        }
    }
}
