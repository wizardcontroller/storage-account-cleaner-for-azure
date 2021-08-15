using Azure.Core;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using Microsoft.AspNetCore.Http;
using com.ataxlab.azure.table.retention.models.extensions;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
// using Microsoft.Azure.Management.Subscription;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using Newtonsoft.Json.Linq;
using com.ataxlab.azure.table.retention.models.models.pagemodel;
using com.ataxlab.azure.table.retention.models.models;
using Newtonsoft.Json;
using System.Diagnostics;
using Azure.Identity;
using Microsoft.Azure.Management.WebSites.Models;

namespace com.ataxlab.azure.table.retention.services.azuremanagement
{
    public interface IAzureManagementAPIClient
    {
        public Task<string> GetSubscriptions();
        public Task<List<SubscriptionDTO>> GetSubscriptionsForLoggedInUser();

        public Task<String> EnsureImpersonationToken();
        Task<List<FunctionEnvelope>> DiscoverAppliancesForSubscriptions(string subscriptionId);
        string GetTenantId(bool isMultitenantShim = true);
    }

    public class AzureManagementAPIClient : IAzureManagementAPIClient
    {
        private HttpClient CurrentHttpClient;
        private SubscriptionClient subscriptionClient;
        public IConfiguration Configuration { get; }
        public HttpContext CurrentHttpContext { get; }
        public ITokenAcquisition TokenAcquisitionHelper { get; }

        ILogger<AzureManagementAPIClient> log;

        public AzureManagementAPIClient(HttpClient httpClient,
            ILogger<AzureManagementAPIClient> logger,
            IHttpContextAccessor httpContextAccessor, IConfiguration config,
            ITokenAcquisition tokenAcquisition)

        {
            CurrentHttpContext = httpContextAccessor.HttpContext;
            Configuration = config;

            TokenAcquisitionHelper = tokenAcquisition;
            log = logger;
            this.CurrentHttpClient = httpClient;
            this.CurrentHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



        }

        public async Task<List<FunctionEnvelope>> DiscoverAppliancesForSubscriptions(string subscriptionId)
        {
            var ret = new List<FunctionEnvelope>();
            // get the apps (and their resource groups) in the subscription
            // as per https://docs.microsoft.com/en-us/rest/api/appservice/webapps/list#code-try-0s
            //GET https://management.azure.com/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/providers/Microsoft.Web/sites?api-version=2019-08-01
            //Authorization: Bearer {token}
            // identify apps with our function signatures
            // GET https://management.azure.com/subscriptions/{tenantid}/resourceGroups/%7BresourceGroupName%7D/providers/Microsoft.Web/sites/%7Bname%7D/functions?api-version=2019-08-01
            // Authorization: Bearer {token}

            // as per https://github.com/Azure/azure-sdk-for-net/blob/Azure.ResourceManager.Compute_1.0.0-preview.2/doc/mgmt_preview_quickstart.md

            //foreach (var subscriptionId in subscriptions)
            {
                var token = await EnsureImpersonationToken();
                var creds = new Microsoft.Rest.TokenCredentials(token);

                Microsoft.Azure.Management.WebSites.WebSiteManagementClient sitesClient =
                      new Microsoft.Azure.Management.WebSites.WebSiteManagementClient(creds);

                Microsoft.Azure.Management.ResourceManager.ResourceManagementClient resourceGroupsClient =
                        new Microsoft.Azure.Management.ResourceManager.ResourceManagementClient(creds);
                try
                {

                    sitesClient.SubscriptionId = subscriptionId;
                    resourceGroupsClient.SubscriptionId = subscriptionId;
                    var response = await resourceGroupsClient.ResourceGroups.ListWithHttpMessagesAsync();
                    if (response != null && response.Body != null)
                    {
                        var responseBody = response.Body;

                        foreach (var group in responseBody)
                        {

                            var resourceGroupName = group.Name;
                            var websiteResponse = await sitesClient.WebApps.ListByResourceGroupWithHttpMessagesAsync(resourceGroupName);

                            var websites = websiteResponse.Body;

                            if (websites != null)
                            {
                                foreach (var site in websites)
                                {
                                    var websiteName = site.Name;
                                    var functionsResponse = await sitesClient.WebApps.ListFunctionsWithHttpMessagesAsync(resourceGroupName, websiteName);

                                    var functions = functionsResponse.Body;
                                    if (functions != null)
                                    {
                                        foreach (var fx in functions)
                                        {
                                            if (fx != null)
                                            {
                                                // TODO match more of our uri templates
                                                // if (IsApplianceByRecognitionStrategy(fx))
                                                {
                                                    ret.Add(fx);
                                                    log.LogInformation($"function config {fx.Config}, kind {fx.Kind}, id {fx.Id}, {fx.Href}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogError($"problem getting functions {e.Message} ");
                }
            }

            return ret;
        }

        private bool IsApplianceByRecognitionStrategy(FunctionEnvelope fx)
        {
            bool ret = false;

            if (fx != null && fx.InvokeUrlTemplate != null)
            {
                ret = fx.InvokeUrlTemplate.ToLower().Contains(ControlChannelConstants.QueryOrchestrationStatusRouteTemplate);
            }

            return ret;
        }

        public async Task<List<SubscriptionDTO>> GetSubscriptionsForLoggedInUser()
        {
            var ret = new List<SubscriptionDTO>();
            try
            {
                log.LogInformation("getting subscriptions for current user");
                var subscriptions = await GetSubscriptions();
                var response = await subscriptions.FromJSONStringAsync<SubscriptionResponse>();
                // decorate the dto with the user who made the request
                if (response != null)
                {
                    foreach (var subscription in response.Subscriptions)
                    {
                        try
                        {
                            subscription.RequestingAzureAdUserOid = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_OID)).FirstOrDefault()?.Value;
                        }
                        catch (Exception e)
                        {
                            log.LogError("problem getting operator azure ad oid during subscriptions retrieval {0}", e.Message);
                        }
                    }

                    ret = response?.Subscriptions;
                }
            }
            catch (Exception e)
            {


                log.LogError("problem deserializing subscriptions {0}", e.Message);
                throw;
            }

            return ret;
        }

        public async Task<bool> IsTokenExpired(string token)
        {
            var ret = false;

            if (token == null)
            {
                return true;
            }

            var handler = new JwtSecurityTokenHandler();

            var toke = handler.ReadJwtToken(token);
            var now = DateTime.UtcNow;
            var minutesPassed = toke.IssuedAt.Subtract(now).Minutes;

            var timeremaining = handler.TokenLifetimeInMinutes + minutesPassed;  // expect negative minutes unless token is not yet valid
            // toke.Claims.Where(c => c.Type.Contains("exp")).FirstOrDefault().Value.FirstOrDefault();
            if (timeremaining <= 15)
            {
                ret = true;
            }

            return await Task.FromResult(ret);
        }

        public async Task<string> EnsureImpersonationToken()
        {
            var isTokenExpiring = await IsTokenExpired(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN));
            string token = string.Empty;

            try
            {
                Microsoft.Identity.Client.AuthenticationResult impersonationResult = null;
                if (String.IsNullOrEmpty(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN)))
                {
                    // never had a token

                    //token = await TokenAcquisitionHelper.GetAccessTokenForUserAsync(
                    //    new List<string>() { "user_impersonation" }, tenantId: this.GetTenantId(), user: this.CurrentHttpContext.User);

                    impersonationResult = await TokenAcquisitionHelper
                        .GetAuthenticationResultForUserAsync(scopes: new List<string>()
                        {ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION}, tenantId: this.GetTenantId());
                    // {ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION}, tenantId: Configuration["AzureAd:TenantId"]);
                    token = impersonationResult.AccessToken;

                    CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN, token);
                }
                else if (await IsTokenExpired(CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN)))
                {
                    //token = await TokenAcquisitionHelper.GetAccessTokenForUserAsync(
                    //    new List<string>() { "user_impersonation" }, tenantId: this.GetTenantId(), user: this.CurrentHttpContext.User);

                    // token is nearing expiration 
                    impersonationResult = await TokenAcquisitionHelper
                                        .GetAuthenticationResultForUserAsync(scopes: new List<string>()
                        {ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION}, tenantId: this.GetTenantId());
                    // {ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION}, tenantId: Configuration["AzureAd:TenantId"]);
                    token = impersonationResult.AccessToken;

                    CurrentHttpContext.Session.SetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN, token);
                }
                else
                {
                    token = CurrentHttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN);

                }
            }
            catch(Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }

            return token;
        }

        private MediaTypeWithQualityHeaderValue GetMediaTypeJson()
        {
            return new MediaTypeWithQualityHeaderValue("application/json");
        }


        public async Task<string> GetSubscriptions()
        {
            string ret = string.Empty;
            var token = string.Empty;
            

            try
            {
                //    if(String.IsNullOrEmpty(HttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN)))
                //    {

                //        impersonationResult = await TokenAcquisitionHelper
                //            .GetAuthenticationResultForUserAsync(scopes: new List<string>()
                //        {ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION}, tenantId: Configuration["AzureAd:TenantId"]);
                //        token = impersonationResult.AccessToken;

                //        HttpContext.Session.SetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN, token);
                //    }
                //    else
                //    {
                //        token = HttpContext.Session.GetString(ControlChannelConstants.SESSION_IMPERSONATION_TOKEN);
                //    }

                // this.HttpContext.Session.SetString(ControlChannelConstants.SESSION_ACCESS_TOKEN, token);
                token = await EnsureImpersonationToken();
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                //var appidClaim = jwtToken.Claims.Where(w => w.Type.Equals("appid")).FirstOrDefault()?.Value;
                // var audClaim = jwtToken.Claims.Where(w => w.Type.Equals("aud")).FirstOrDefault()?.Value;
                //string authorityUri = "https://login.windows.net/common/oauth2/authorize";
                //AuthenticationContext authContext = new AuthenticationContext(authorityUri);
                //var authenticationResult = 
                //    await authContext.AcquireTokenAsync(ControlChannelConstants.USERIMPERSONATION,
                //            new ClientAssertion(appidClaim, token)); 

                this.CurrentHttpClient.DefaultRequestHeaders.Clear();

                this.CurrentHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // .Add("Authorization", "Bearer " + token);
                this.CurrentHttpClient.DefaultRequestHeaders.Add("Host", "management.azure.com");
                this.CurrentHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(HttpMethod.Get, "/subscriptions?api-version=2020-01-01");
                log.LogInformation("request message configured for management rest {0}", this.CurrentHttpClient.BaseAddress.AbsoluteUri.ToString());

                var content = string.Empty;
                try
                {
                    var response = await this.CurrentHttpClient.SendAsync(request);

                    response.EnsureSuccessStatusCode();

                    content = await response.Content.ReadAsStringAsync();

                }
                catch(Exception e)
                {
                    log.LogError($"exception getting subscriptions {e.Message}");
                    throw;
                }

                return content;
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                // await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION }, ex.MsalUiRequiredException);
                // return string.Empty;

                log.LogError(ex.Message);
                throw;
            }
            catch (MsalUiRequiredException ex)
            {
                // await TokenAcquisitionHelper.ReplyForbiddenWithWwwAuthenticateHeaderAsync(new string[] { ControlChannelConstants.AZUREMANAGEMENT_USERIMPERSONATION }, ex);
                //return string.Empty;

                log.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;

            }

            return ret;
        }

        /// <summary>
        /// support single tenant and multi tenant callers
        /// 
        /// single tenant - tenant id returned from app config
        /// multi tenant - app config tenant id is set to organizations, return tenant 
        /// from issuer sts url
        /// </summary>
        /// <returns></returns>
        public string GetTenantId(bool isMultitenantShim = false)
        {
            var tenantId = String.Empty;
            log.LogInformation("getting tenant id from user claims");
            var configuredTenantId = Configuration["AzureAd:TenantId"];

            //if(isMultitenantShim)
            //{
            //    // return tenant id from claims
            //     var stsUrl = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains("identityprovider")).FirstOrDefault()?.Value;
            //    var splitStsUrl = stsUrl.Split("https://sts.windows.net/");
            //    tenantId = splitStsUrl.LastOrDefault().TrimEnd('/');
            //    return tenantId;
            //}

            //if(configuredTenantId.Contains("organizations"))
            //{
            //    // return tenant id from claims
            //    // var tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains(ControlChannelConstants.CLAIM_TENANT_UTID)).FirstOrDefault()?.Value;
            //    var stsUrl = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Contains("identityprovider")).FirstOrDefault()?.Value;
            //    log.LogInformation($"sts url = {stsUrl}");
            //    var splitStsUrl = stsUrl.Split("https://sts.windows.net/");
            //    tenantId = splitStsUrl.LastOrDefault().TrimEnd('/');
            //}
            //else
            //{
            //    log.LogInformation($"using configured tenantid");
            //    // single tenant return tenantid from configuration
            //    tenantId = configuredTenantId;
            //}

            foreach(var c in this.CurrentHttpContext.User.Claims)
            {
                log.LogInformation($"claim type is {c.Type}");
                log.LogInformation($"claim Value is {c.Value}");
                log.LogInformation($"claim Issuer is {c.Issuer}");

            }
            tenantId = this.CurrentHttpContext.User.Claims.Where(c => c.Type.ToLowerInvariant().Equals("tid")).FirstOrDefault()?.Value;

            return tenantId;
        }


    }
}
