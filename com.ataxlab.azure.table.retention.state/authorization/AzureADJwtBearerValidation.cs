using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace com.ataxlab.azure.table.retention.services.authorization
{
    public class AzureADJwtBearerValidation
    {
        private IConfiguration _configuration;
        private const string scopeType = @"http://schemas.microsoft.com/identity/claims/scope";
        private ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private ClaimsPrincipal _claimsPrincipal;

        public string _tenantId { get; set; } // string.Empty;

        public string _audience { get; set; } // string.Empty;

        public string _instance { get; set; } // string.Empty;
        private string _requiredScope = "http://schemas.microsoft.com/identity/claims/scope";

        private string _wellKnownEndpointStr = string.Empty;
        public string _wellKnownEndpoint { get { return _instance + _tenantId + "/v2.0/.well-known/openid-configuration"; } }

        public AzureADJwtBearerValidation(IConfiguration configuration, ILogger<AzureADJwtBearerValidation> logger)
        {
            _configuration = configuration;
            //_log = logger;
            // _tenantId = _configuration["AzureAd:TenantId"];
            _tenantId = _configuration["AzureAd:TenantId"] == null || _configuration["AzureAd:TenantId"] == string.Empty ?
                        _configuration["AzureAd__TenantId"] : _configuration["AzureAd:TenantId"];

            // _audience = _configuration["AzureAd:ClientId"];
            _audience = _configuration["AzureAd:ClientId"] == null || _configuration["AzureAd:ClientId"] == string.Empty ?
                        _configuration["AzureAd__ClientId"] : _configuration["AzureAd:ClientId"];

            // _instance = _configuration["AzureAd:Instance"];
            _instance = _configuration["AzureAd:Instance"] == null || _configuration["AzureAd:Instance"] == string.Empty ?
                        _configuration["AzureAd__Instance"] : _configuration["AzureAd:Instance"];
            //  _wellKnownEndpoint = $"{_instance}{_tenantId}/v2.0/.well-known/openid-configuration";

            // _wellKnownEndpoint = $"{_instance}{_tenantId}/v2.0/.well-known/openid-configuration";
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string authorizationHeader)
        {

            try
            {
                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    return null;
                }

                if (!authorizationHeader.Contains("Bearer"))
                {
                    return null;
                }

                var accessToken = authorizationHeader.Substring("Bearer ".Length);

                var oidcWellknownEndpoints = await GetOIDCWellknownConfiguration();

                var tokenValidator = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = true,
                    ValidAudience = _audience,
                    ValidateAudience = true,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKeys = oidcWellknownEndpoints.SigningKeys,
                    ValidIssuer = oidcWellknownEndpoints.Issuer
                };

                try
                {
                    SecurityToken securityToken;
                    _claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);

                    if (IsScopeValid(_requiredScope))
                    {
                        return _claimsPrincipal;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    int i = 0;
                    //_log.LogError(ex.ToString());
                }
            }
            catch(Exception e)
            {
                //_log.LogError("error performing authentication {0}", e.Message);
            }

            return null;
        }

        public string GetPreferredUserName()
        {
            string preferredUsername = string.Empty;
            var preferred_username = _claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "preferred_username");
            if (preferred_username != null)
            {
                preferredUsername = preferred_username.Value;
            }

            return preferredUsername;
        }

        private async Task<OpenIdConnectConfiguration> GetOIDCWellknownConfiguration()
        {
            //_log.LogDebug($"Get OIDC well known endpoints {_wellKnownEndpoint}");
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                 _wellKnownEndpoint, new OpenIdConnectConfigurationRetriever());

            return await _configurationManager.GetConfigurationAsync();
        }

        private bool IsScopeValid(string scopeName)
        {
            if (_claimsPrincipal == null)
            {
                //_log.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            var scopeClaim = _claimsPrincipal.HasClaim(x => x.Type == scopeType)
                ? _claimsPrincipal.Claims.First(x => x.Type == scopeType).Value
                : string.Empty;

            if (string.IsNullOrEmpty(scopeClaim))
            {
                //_log.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            // TODO
            // add other filter conditions here
            //if (!scopeClaim.Equals(scopeName, StringComparison.OrdinalIgnoreCase))
            //{
            //    // _log.LogWarning($"Scope invalid {scopeName}");
            //    return false;
            //}

            //_log.LogDebug($"Scope valid {scopeName}");
            return true;
        }
    }
}
