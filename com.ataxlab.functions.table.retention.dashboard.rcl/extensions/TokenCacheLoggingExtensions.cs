﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.extensions
{
    public static class TokenCacheLoggingExtensions
    {
        public static void ReadFromCacheFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Reading from cache failed", exp);
        }

        public static void WriteToCacheFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Writing to cache failed", exp);
        }

        public static void TokenCacheCleared(this ILogger logger, string userId)
        {
            logger.LogInformation("Cleared token cache for User: {0}", userId);
        }

        public static void TokensRetrievedFromStore(this ILogger logger, string key)
        {
            logger.LogTrace("Retrieved all tokens from store for Key: {0}", key);
        }

        public static void TokensWrittenToStore(this ILogger logger, string clientId, string userId, string resource)
        {
            logger.LogTrace("Token states changed for Client: {0} User: {1}  Resource: {2} writing all tokens back to store", clientId, userId, resource);
        }
    }
}