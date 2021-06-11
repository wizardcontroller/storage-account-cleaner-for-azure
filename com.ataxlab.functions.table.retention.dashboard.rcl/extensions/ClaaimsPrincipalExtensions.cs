using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.extensions
{
    public static class AzureAdClaimTypes
    {
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public const string Scope = "http://schemas.microsoft.com/identity/claims/scope";
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType, bool throwIfNotFound = false)
        {
            string value = principal.FindFirst(claimType)?.Value;
            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, "The supplied principal does not contain a claim of type {0}", claimType));
            }

            return value;
        }

        public static string GetObjectIdentifierValue(this ClaimsPrincipal principal, bool throwIfNotFound = true)
        {
            return principal.FindFirstValue(AzureAdClaimTypes.ObjectId, throwIfNotFound);
        }
    }
}
