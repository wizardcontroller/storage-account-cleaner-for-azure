
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using com.ataxlab.azure.table.retention.state.entities;
using com.ataxlab.functions.table.retention.entities;
using com.ataxlab.functions.table.retention.utility;
using Dynamitey.DynamicObjects;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.OData.UriParser;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace com.ataxlab.functions.table.retention.services
{


    public interface ITableRetentionApplianceActivities
    {
        Task<TableStorageTableRetentionPolicyEnforcementResultEntity> ApplyTableStorageTableRetentionPolicy(string authToken, StorageAccountEntity acct, TableStorageTableRetentionPolicyEntity policy);
        Task<TableStorageEntityRetentionPolicyEnforcementResultEntity> ApplyTableStorageEntityRetentionPolicy(string authToken, StorageAccountEntity acct, TableStorageEntityRetentionPolicyEntity policy, Func<string> tickProvider);
        Task<TableStorageEntityRetentionPolicyEntity> BuildTableStorageEntityRetentionPolicy(int tableEntityRetentionInDays, List<string> candidateTableNames);
        Task<TableStorageTableRetentionPolicyEntity> BuildTableStorageTableRetentionPolicy(int tableDeletionAgeInMonths, List<string> candidateTableNames);
        Task<bool> CanGetStorageAccountsForUser(string tenantId, string oid, string impersonate, ApplianceSessionContextEntityBase applianceContext);
        Task<int> DeleteOldDiagnosticsEntitiesForTable(string ticks, CloudTable cloudTable);
        List<string> GetCandidateMatches(List<string> tableNames, List<string> defaultTableMatchPatterns);
        Task<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> RenderRetentionPolicyTuple(string authToken, StorageAccountEntity storageAccount, ApplianceSessionContextEntity applianceContext);
        Task<List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>> RenderRetentionPolicyTuples(string authToken, List<StorageAccountEntity> v2StorageAccounts, ApplianceSessionContextEntity applianceContext);

        Task<List<StorageAccountEntity>> GetStorageAccountsForUser(string tenantId, string oid, string impersonate, ApplianceSessionContextEntityBase ctx);

        // CloudTable GetStorageTableReference(CloudTableClient client, string tableName);
        Task<List<string>> GetTableNames(TableServiceClient tableClient);

        Task<ApplianceSessionContextEntity> InitializeCurrentJobOutput(EntityId entityId, IDurableEntityClient entityClient, List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> tuples);
        Task<Task<TSignal>> WhenAnySignalledTask<TSignal>(List<Task<TSignal>> tasks) where TSignal : class;
        Task<List<String>> DeleteOldTables(string authToken, StorageAccountEntity storageAccount, TableStorageTableRetentionPolicyEntity policy);
        Task<int> AuditOldDiagnosticsEntitiesForTable(string ticks, CloudTable cloudTable);
    }

    public class TableRetentionApplianceActivities : ITableRetentionApplianceActivities
    {
        private ILogger<TableRetentionApplianceActivities> log;

        public TableRetentionApplianceActivities(ILogger<TableRetentionApplianceActivities> logger)
        {
            this.log = logger;
            log.LogInformation("constructor completed");
        }

        /// <summary>
        /// supply a list of signals you want to listen for
        /// returns the awaitable winner
        /// </summary>
        /// <typeparam name="TSignal"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public async Task<Task<TSignal>> WhenAnySignalledTask<TSignal>(List<Task<TSignal>> tasks) where TSignal : class
        {
            var gate = await Task.WhenAny<TSignal>(tasks);

            log.LogInformation("signal processor is returning a signal");
            return gate;
        }

        private async Task<StorageAccountEntity> GetStorageAccountForAccess(string authToken, string accountId, ApplianceSessionContextEntityBase applianceContext)
        {
            var ret = new StorageAccountEntity();

            try
            {
                var creds = new Microsoft.Rest.TokenCredentials(authToken);
                Microsoft.Azure.Management.Storage.StorageManagementClient storage = new Microsoft.Azure.Management.Storage.StorageManagementClient(creds);

                if (storage == null)
                {
                    log.LogTrace("failed to get storage account for access");
                    return ret;
                }

                storage.SubscriptionId = applianceContext.SelectedSubscriptionId;
                var accountsRes = await storage.StorageAccounts.ListAsync();
                // validate the subscription is correct
                if (accountsRes.Count() > 0)
                {
                    var accounts = await storage.StorageAccounts.ListAsync();
                    var targetAccount = accounts.Where(a => a.Id.Equals(accountId)).FirstOrDefault();
                    var resourceGroup = await this.GetResourceGroupFromStorageAccountId(targetAccount.Id);
                    ret.Id = targetAccount.Id;
                    ret.Name = targetAccount.Name;
                    ret.SkuName = targetAccount.Sku.Name;
                    ret.StorageAccountKind = targetAccount.Kind;
                    ret.TenantId = applianceContext.TenantId;
                    var keys = await storage.StorageAccounts.ListKeysAsync(resourceGroup, targetAccount.Name);
                    ret.Key = keys.Keys[0].Value;
                    log.LogTrace("returning a storage account key");

                    ret.PrimaryTableStorageEndpoint = new Uri(targetAccount.PrimaryEndpoints.Table);
                    log.LogTrace("returning storage account for access");
                    return ret;
                }
                else
                {
                    log.LogWarning("failure to match appliance context with accessed storage account");
                    log.LogTrace("storage subscription mismatch. appliance context subcriptionid is {0}, storage subscription id is {1}", applianceContext.SelectedSubscriptionId, storage.SubscriptionId);
                }
            }
            catch (Exception e)
            {
                log.LogWarning("failed to get stoarge account for access with error {0}", e.Message);
            }

            return ret;
        }

        /// <summary>
        /// apply regex [\-a-zA-Z0-9_\.]*
        /// template 
        /// /subscriptions/subscriptionguid/resourceGroups/resourcegroupname/providers/Microsoft.Storage/storageAccounts/accountName
        /// expect resourcegroup name in match 8
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<string> GetResourceGroupFromStorageAccountId(string id)
        {
            // Regex rx = new Regex(@"[\-a-zA-Z0-9_\.]*/gm",RegexOptions.Compiled);
            Regex rx = new Regex(@"[\-a-zA-Z0-9_\.]*", RegexOptions.Compiled);
            MatchCollection matches = rx.Matches(id);
            if (matches.Count > 8)
            {
                return await Task.FromResult(matches[7].Value);
            }

            return await Task.FromResult(String.Empty);
        }

        public async Task<List<StorageAccountEntity>> GetStorageAccountsForUser(string tenantId,
                                                string oid,
                                                string impersonate,
                                                ApplianceSessionContextEntityBase ctx)
        {
            var ret = new List<StorageAccountEntity>();
            try
            {
                log.LogTrace("testing USERTOKEN with subscription {0}", ctx.SelectedSubscriptionId);

                var creds = new Microsoft.Rest.TokenCredentials(impersonate);
                Microsoft.Azure.Management.Storage.StorageManagementClient storage = new Microsoft.Azure.Management.Storage.StorageManagementClient(creds);


                storage.SubscriptionId = ctx.SelectedSubscriptionId;

                log.LogInformation("testing USERTOKEN StorageManagementClient for subscription {0}", storage?.SubscriptionId);
                var storageAccounts = await storage?.StorageAccounts?.ListAsync();
                if (storageAccounts == null || storageAccounts.Count() == 0)
                {
                    log.LogWarning("failed to access any storage accounts with the supplied token");

                }
                else
                {
                    // todo make this more robust
                    // validate tenant id
                    foreach (var acct in storageAccounts.Where(w => w.Kind.ToLower().Contains("storagev2")))
                    {
                        try
                        {
                            ////if (acct.sub.Contains(ctx.SelectedSubscriptionId))
                            //{
                            log.LogWarning("retrieved storage account  exists in selected subscription");
                            ret.Add(new StorageAccountEntity()
                            {
                                Id = acct.Id,
                                Name = acct.Name,
                                Location = acct.Location,
                                SkuName = acct.Sku.Name,
                                StorageAccountKind = acct.Kind,
                                RequestingAzureAdUserOid = oid,
                                TenantId = tenantId,
                                SubscriptionId = ctx.SelectedSubscriptionId
                            });
                            //}
                        }
                        catch (Exception e)
                        {
                            log.LogError($"problem getting storage account for user {acct.Id}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting storage accounts");
            }


            return await Task.FromResult<List<StorageAccountEntity>>(ret);
        }

        /// <summary>
        /// initializes strong workflow dependency on tuple of
        /// storage account and retention policy
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityClient"></param>
        /// <param name="tuples"></param>
        /// <returns></returns>
        public async Task<ApplianceSessionContextEntity> InitializeCurrentJobOutput(EntityId entityId, IDurableEntityClient entityClient,
                            List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> tuples)
        {
            ApplianceSessionContextEntity ret = new ApplianceSessionContextEntity();
            try
            {
                log.LogInformation("updating appliance state");
                //var currentState = await entityClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(entityId);

                //var currentOutput = await currentState.EntityState.GetCurrentJobOutput();
                //await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(entityId,
                //            proxy =>
                //            {

                //                // reset the current job output
                //                proxy.SetCurrentJobOutput(new ApplianceJobOutputEntity());
                //            });

                log.LogInformation("initializing job - updating current job output retention policy tuples");
                //currentOutput.RetentionPolicyTuples.AddRange(tuples);

                await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(entityId,
                            proxy =>
                            {

                                // update the job state with the
                                // calculated policy tuples
                                proxy.InitializeCurrentJobOutput(tuples);
                            });

                var retState = await entityClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(entityId);
                ret = retState.EntityState;

            }
            catch (Exception e)
            {
                log.LogError("problem updating appliance state {0}", e.Message);
            }

            return ret;
        }

        public async Task<bool> CanGetStorageAccountsForUser(string tenantId,
                                                        string oid,
                                                        string impersonate,
                                                        ApplianceSessionContextEntityBase ctx)
        {
            var ret = false;
            try
            {
                log.LogTrace("testing USERTOKEN with subscription {0}", ctx.SelectedSubscriptionId);

                var creds = new Microsoft.Rest.TokenCredentials(impersonate);
                Microsoft.Azure.Management.Storage.StorageManagementClient storage = new Microsoft.Azure.Management.Storage.StorageManagementClient(creds);


                storage.SubscriptionId = ctx.SelectedSubscriptionId;

                log.LogInformation("testing USERTOKEN StorageManagementClient for subscription {0}", storage?.SubscriptionId);
                var storageAccounts = await storage?.StorageAccounts?.ListAsync();
                if (storageAccounts == null || storageAccounts.Count() == 0)
                {
                    log.LogWarning("failed to access any storage accounts with the supplied token");

                }
                else
                {
                    // validate tenant id
                    foreach (var acct in storageAccounts)
                    {
                        // storage account ids contain subscription ids
                        if (!acct.Id.Contains(ctx.SelectedSubscriptionId))
                        {
                            log.LogWarning("retrieved storage account does not exist in selected subscription");
                            return false;
                        }


                    }



                    List<bool> testResults = new List<bool>();
                    foreach (var acct in ctx.SelectedStorageAccounts)
                    {
                        // adds false to the list if id missing
                        testResults.Add(storageAccounts.Where(s => s.Id.Equals(acct.Id)).Any());
                    }

                    // result is false if any of the test results were false
                    ret = !testResults.Where(w => w == false).Any();
                    log.LogTrace("storage accounts found {0}", storageAccounts?.Count());
                }
            }
            catch (Exception e)
            {
                log.LogError("problem getting storage accounts");
            }

            return await Task.FromResult<bool>(ret);
        }

        /// <summary>
        /// public interface - do not populate secrets
        /// except the auth token that will be used to retrieve them
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="v2StorageAccounts"></param>
        /// <returns></returns>
        public async Task<List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>> RenderRetentionPolicyTuples(string authToken, List<StorageAccountEntity> v2StorageAccounts, ApplianceSessionContextEntity applianceContext)
        {
            var ret = new List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>();
            // var ret = new List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>();

            try
            {
                foreach (var storageAccount in v2StorageAccounts)
                {
                    try
                    {
                        // storage account secrets are populated just in time
                        var account = await this.GetStorageAccountForAccess(authToken, storageAccount.Id, applianceContext);
                        Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity> newTuple = await RenderRetentionPolicyTuple(authToken, account, applianceContext);

                        ret.Add(newTuple);

                        log.LogInformation("got retention policy tuple for storage account");
                    }
                    catch (Exception e)
                    {

                        log.LogError("problem getting storage account tuple {0}", e.Message);

                    }
                }

            }
            catch (Exception e)
            {

                log.LogError("problem getting storage account tuple {0}", e.Message);
            }
            log.LogTrace(string.Format("Got All V2 Storage Accounts tuples"));
            return ret;
        }

        /// <summary>
        /// render the retention surface for a storage account
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="storageAccount"></param>
        /// <param name="applianceContext"></param>
        /// <returns></returns>
        public async Task<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> RenderRetentionPolicyTuple(string authToken, StorageAccountEntity storageAccount, ApplianceSessionContextEntity applianceContext)
        {
            // var tableNames = await this.GetTableNamesInStorageAccount(storageAccount);
            // var defaultRetentionConfiguration = new azure.table.retention.models.DefaultTableRetentionConfiguration();
            var retentionPolicy = new TableStorageRetentionPolicyEntity();
            //var tableRetentionPolicy = new TableStorageTableRetentionPolicyEntity();
            //var entityRetentionPolicy = new TableStorageEntityRetentionPolicyEntity();
            try
            {
                var currentMetricsRetentionPolicy = await applianceContext.GetCurrentMetricsRetentionPolicy(storageAccount);
                var currentDiagnosticsRetentionPolicy = await applianceContext.GetCurrentDiagnosticsRetentionPolicy(storageAccount);

                //List<string> subjectDiagnosticsTables = currentDiagnosticsRetentionPolicy.TableNames; // this.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADDIagnosticsTableNameMatchPatterns);

                // calculate table names for the provided date range
                var metricsRetentionSurface = await this.AuditMetricsRetentionSurface(authToken, storageAccount, currentMetricsRetentionPolicy); // this.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADMetricsTableNameMatchPatterns);
                currentMetricsRetentionPolicy.MetricRetentionSurface = metricsRetentionSurface;

                if (currentDiagnosticsRetentionPolicy.DiagnosticsRetentionSurface == null ||
                    currentDiagnosticsRetentionPolicy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities == null ||
                    currentDiagnosticsRetentionPolicy.TableNames == null ||
                    currentDiagnosticsRetentionPolicy.TableNames.Count == 0)
                {
                    // here because we must render the reference data for WAD, LAD et al
                    currentDiagnosticsRetentionPolicy.DiagnosticsRetentionSurface = new DiagnosticsRetentionSurfaceEntity();
                    currentDiagnosticsRetentionPolicy.DiagnosticsRetentionSurface.InitializeDiagnosticsRetentionSurface();
                }
                var diagnosticsRetentionSurface = await this.AuditDiagnosticsRetentionSurface(authToken, storageAccount, currentDiagnosticsRetentionPolicy);
                currentDiagnosticsRetentionPolicy.DiagnosticsRetentionSurface = diagnosticsRetentionSurface;

                //int tableDeletionAgeInMonths = currentMetricsRetentionPolicy.DeleteOlderTablesThanCurrentMonthMinusThis; //  2;
                //int entityDeletionAgeInDays = currentDiagnosticsRetentionPolicy.NumberOfDays; // 1;

                log.LogWarning("deleting windows azure metrics tables older than {0} months old", currentMetricsRetentionPolicy.DeleteOlderTablesThanCurrentMonthMinusThis);
                log.LogWarning("deleting windows azure diagnostics table entities older than {0} months old", currentDiagnosticsRetentionPolicy.NumberOfDays);
                log.LogWarning($"found { diagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities.Where(w => w.ItemExists == true).Count()} diagnostics tables in storage account");

                //tableRetentionPolicy = await this.BuildTableStorageTableRetentionPolicy(tableDeletionAgeInMonths, 
                //                        subjectMetricsTables.MetricsRetentionSurfaceItemEntities.Select(s => s.TableName).ToList());
                //entityRetentionPolicy = await this.BuildTableStorageEntityRetentionPolicy(entityDeletionAgeInDays, subjectDiagnosticsTables);

                retentionPolicy = new TableStorageRetentionPolicyEntity()
                {
                    Id = Guid.NewGuid(),
                    TableStorageEntityRetentionPolicy = currentDiagnosticsRetentionPolicy,
                    TableStorageTableRetentionPolicy = currentMetricsRetentionPolicy
                };

            }
            catch (Exception e)
            {
                log.LogError("problem getting  table storage retention policy {0}", e.Message);
            }

            var newTuple = new Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>(retentionPolicy, storageAccount);
            return newTuple;

        }

        [Obsolete]
        private async Task<List<string>> GetTableNamesInStorageAccount(StorageAccountEntity account)
        {
            TableServiceClient tableClient = GetTableServiceClient(account);

            List<string> existingTableNames = await GetTableNames(tableClient);

            return existingTableNames;
        }

        [Obsolete]
        private TableServiceClient GetTableServiceClient(StorageAccountEntity account)
        {
            // TODO - clean up defensive approaches
            string key = account.Key;
            Uri primaryEndpoint = account.PrimaryTableStorageEndpoint;
            string accountName = account.Name;
            string connectionString = account.ConnectionString;

            var tableClient = new TableServiceClient(primaryEndpoint, new TableSharedKeyCredential(accountName, key));
            return tableClient;
        }


        /// <summary>
        /// todo - init this from token instead of getting it downstream
        /// from a client that might be null 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private CloudTable GetStorageTableReference(CloudTableClient client, String tableName)
        {
            try
            {
                CloudTable cloudTable = client.GetTableReference(tableName);
                return cloudTable;
            }
            catch (Exception e)
            {
                log.LogError("problem getting cloud table client");
            }

            return null;
        }

        [Obsolete]
        public async Task<List<string>> GetTableNames(TableServiceClient tableClient)
        {
            var existingTableNames = new List<String>();
            var tables = tableClient.GetTablesAsync();
            var asyncEnunerator = tables.AsPages().GetAsyncEnumerator();
            while (await asyncEnunerator.MoveNextAsync())
            {
                Azure.Page<TableItem> currentTable = asyncEnunerator.Current;
                foreach (var item in currentTable.Values)
                {
                    existingTableNames.Add(item.TableName);
                }
            }

            return existingTableNames;
        }

        [Obsolete]
        public List<string> GetCandidateMatches(List<string> tableNames, List<string> defaultTableMatchPatterns)
        {
            List<String> matchedTables = new List<string>();
            // identify WAD tables in the tables in this storage acount
            // i admit it - i tried an failed to express this in join notation in less than 10 seconds 
            matchedTables.AddRange(tableNames.Where(w => defaultTableMatchPatterns.Any(a => w.Contains(a))).ToList());
            return matchedTables;
        }

        public async Task<TableStorageTableRetentionPolicyEnforcementResultEntity> ApplyTableStorageTableRetentionPolicy(string authToken,
            StorageAccountEntity acct, TableStorageTableRetentionPolicyEntity policy)
        {
            var ret = new TableStorageTableRetentionPolicyEnforcementResultEntity() { Id = Guid.NewGuid(), Policy = policy };
            log.LogInformation("applying policy");
            try
            {
                var tokenCredentials = new TokenCredential(authToken);
                var storageCreds = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(acct.Name, acct.Key);
                var storageTableClient = new CloudTableClient(acct.PrimaryTableStorageEndpoint, storageCreds);
                if (policy.PolicyEnforcementMode == PolicyEnforcementMode.ApplyPolicy)
                {
                    var retentionSurface = await this.AuditMetricsRetentionSurface(authToken, acct, policy);

                    ret.PolicyTriggerCount += retentionSurface.MetricsRetentionSurfaceItemEntities.Where(w => w.ItemWillBeDeleted).Count();
                    ret.Policy.MetricRetentionSurface = retentionSurface;
                }
                else
                {
                    var retentionSurface = await this.AuditMetricsRetentionSurface(authToken, acct, policy);
                    ret.PolicyTriggerCount += retentionSurface.MetricsRetentionSurfaceItemEntities
                                                .Where(w => w.ItemExists == true).Count();
                    ret.Policy.MetricRetentionSurface = retentionSurface;
                }

                log.LogTrace("returning deleted table names");
            }
            catch (Exception e)
            {
                log.LogError("problem returning deleted table names {0}", e.Message);
            }

            return await Task.FromResult<TableStorageTableRetentionPolicyEnforcementResultEntity>(ret);
        }




        public async Task<TableStorageEntityRetentionPolicyEnforcementResultEntity> ApplyTableStorageEntityRetentionPolicy(
            string authToken, StorageAccountEntity acct, TableStorageEntityRetentionPolicyEntity policy, Func<String> tickProvider)
        {
            var ret = new TableStorageEntityRetentionPolicyEnforcementResultEntity() { Id = Guid.NewGuid(), Policy = policy };
            var storageCreds = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(acct.Name, acct.Key);
            var storageTableClient = new CloudTableClient(acct.PrimaryTableStorageEndpoint, storageCreds);

            try
            {
                foreach (var tableName in policy.TableNames)
                {
                    try
                    {
                        var storageTable = this.GetStorageTableReference(storageTableClient, tableName);

                        // support WHATIF
                        if (policy.PolicyEnforcementMode == entities.PolicyEnforcementMode.ApplyPolicy)
                        {
                            log.LogWarning("applying policy to delete data");
                            ret.PolicyTriggerCount += await this.DeleteOldDiagnosticsEntitiesForTable(policy.GetTicks(tickProvider), storageTable);
                            log.LogInformation("policy has deleted old data");
                        }
                        else if (policy.PolicyEnforcementMode == entities.PolicyEnforcementMode.WhatIf)
                        {
                            log.LogInformation("running policy in WHATIF mode");
                            ret.PolicyTriggerCount += await this.AuditOldDiagnosticsEntitiesForTable(policy.GetTicks(tickProvider), storageTable);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError("problem applying policy {0}", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError("problem deleting old entities {0}", e.Message);
            }

            log.LogInformation("done applying policy");
            return ret;
        }

        [Obsolete]
        public async Task<TableStorageTableRetentionPolicyEntity> BuildTableStorageTableRetentionPolicy(int tableDeletionAgeInMonths, List<String> candidateTableNames)
        {

            // render TableRetentionPolicy
            TableStorageTableRetentionPolicyEntity tableRetentionPolicy = new TableStorageTableRetentionPolicyEntity()
            {
                Id = Guid.NewGuid(),
                // TableNames = candidateTableNames,
                DeleteOlderTablesThanCurrentMonthMinusThis = tableDeletionAgeInMonths
            };

            return await Task.FromResult<TableStorageTableRetentionPolicyEntity>(tableRetentionPolicy);
        }

        [Obsolete]
        public async Task<TableStorageEntityRetentionPolicyEntity> BuildTableStorageEntityRetentionPolicy(int tableEntityRetentionInDays, List<String> candidateTableNames)
        {
            TableStorageEntityRetentionPolicyEntity policy = new TableStorageEntityRetentionPolicyEntity()
            {
                Id = Guid.NewGuid(),
                NumberOfDays = tableEntityRetentionInDays,

            };

            return await Task.FromResult<TableStorageEntityRetentionPolicyEntity>(policy);
        }

        public async Task<MetricRetentionSurfaceEntity> AuditMetricsRetentionSurface(string authToken, StorageAccountEntity storageAccount, TableStorageTableRetentionPolicyEntity policy)
        {
            // as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
            var ret = new MetricRetentionSurfaceEntity();

            // render the date range strings for the associated table names
            var suffixCount = await policy.InitializeWADMetricsTableNames();

            log.LogInformation("calculating table names");
            try
            {

                if (authToken == null || storageAccount == null || policy == null)
                {
                    return ret;
                }

                var tokenCredentials = new TokenCredential(authToken);
                var cred = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(authToken);

                var storageCreds = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.Key);
                var cloudTableClient = new CloudTableClient(storageAccount.PrimaryTableStorageEndpoint, storageCreds);

                var metricTables = new List<String>();

                try
                {
                    metricTables = await GetWadMetricsTablesInStorageAccount(policy, cloudTableClient);
                    if (metricTables.Count > 0)
                    {
                        var matches = metricTables.Where(metricTableName => policy.TableNames.Contains(metricTableName)).ToList();
                        var exclusions = metricTables.Where(metricTableName => !(policy.TableNames.Contains(metricTableName))).ToList();

                        // table names in storage match table names represented by the policy
                        Parallel.ForEach(matches, async tableName =>
                        {
                            var newEntity = new MetricsRetentionSurfaceItemEntity()
                            {
                                Id = Guid.NewGuid(),
                                DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage"),
                                ItemDescription = "WADMetrics: Standard prefix for all WADMetrics tables",
                                ItemExists = true,
                                ItemType = RetentionSurfaceItemDescriptor.IsMetricsTableItem,
                                ItemWillBeDeleted = true,
                                PolicyAgeTriggerInMonths = policy.DeleteOlderTablesThanCurrentMonthMinusThis,
                                StorageAccountId = storageAccount.Id,
                                SuscriptionId = storageAccount.SubscriptionId,
                                TableName = tableName
                            };

                            if (policy.PolicyEnforcementMode == PolicyEnforcementMode.ApplyPolicy)
                            {
                                try
                                {
                                    var tableRef = cloudTableClient.GetTableReference(tableName);
                                    var deleteResult = await tableRef.DeleteIfExistsAsync();
                                    newEntity.IsDeleted = deleteResult;
                                }
                                catch(Exception e)
                                {
                                    log.LogError($"problem deleting table: {tableName} - {e.Message}");
                                    newEntity.IsDeleted = false;
                                }
                            }

                            ret.MetricsRetentionSurfaceItemEntities.Add(newEntity);


                            log.LogInformation(tableName + " found");
                        });

                        // table names in storage not represented by this policy
                        Parallel.ForEach(exclusions, excludedTableName =>
                        {
                            ret.MetricsRetentionSurfaceItemEntities.Add(new MetricsRetentionSurfaceItemEntity()
                            {
                                Id = Guid.NewGuid(),
                                DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage"),
                                ItemDescription = "WADMetrics: Standard prefix for all WADMetrics tables",
                                ItemExists = true,
                                ItemType = RetentionSurfaceItemDescriptor.IsMetricsTableItem,
                                ItemWillBeDeleted = false,
                                PolicyAgeTriggerInMonths = policy.DeleteOlderTablesThanCurrentMonthMinusThis,
                                StorageAccountId = storageAccount.Id,
                                SuscriptionId = storageAccount.SubscriptionId,
                                TableName = excludedTableName,
                                IsDeleted = false
                            });

                            log.LogInformation(excludedTableName + " found");


                        });
                    }
                }
                catch (Exception e)
                {
                    log.LogError($"exception auditing metric retention surface {e.Message}");
                }



            }
            catch (Exception e)
            {
                log.LogError($"exception auditing metric retention surface {e.Message}");
            }

            return ret;
        }

        public async Task<MetricRetentionSurfaceEntity> DeprecatedAuditMetricsRetentionSurface(string authToken, StorageAccountEntity storageAccount, TableStorageTableRetentionPolicyEntity policy)
        {
            // as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
            string[] aggregationPrefixes = policy.MetricRetentionSurface.AggregationPrefixes; // { "PT1H", "PT1M" };
            var ret = new MetricRetentionSurfaceEntity();
            log.LogInformation("calculating table names");
            try
            {

                if (authToken == null || storageAccount == null || policy == null)
                {
                    return ret;
                }

                var tokenCredentials = new TokenCredential(authToken);
                var cred = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(authToken);
                var cloudTableClient = new CloudTableClient(storageAccount.PrimaryTableStorageEndpoint, cred);

                // 40 mths = 40/12 TODO validate rounding error captures remainder in months
                var yearCount = policy.DeleteOlderTablesThanCurrentMonthMinusThis / 12 < 1 ? 1 : policy.DeleteOlderTablesThanCurrentMonthMinusThis / 12;

                List<DateTime> years = new List<DateTime>();
                for (var count = yearCount; count >= 0; count--)
                {
                    // subtract x number of years from now
                    var year = DateTime.Now.Subtract(new TimeSpan(365 * count, 0, 0, 0));
                    years.Add(year);
                }

                //do
                //{
                List<string> wadMetricsTables = await GetWadMetricsTablesInStorageAccount(policy, cloudTableClient);

                // as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
                // and https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
                // and https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage
                var order = years.OrderBy(o => o.Date).ToList();
                foreach (var year in years.OrderBy(o => o.Date).ToList())
                {
                    string counterYear = year.ToString("yyyy"); // DateTime.Now.ToString("yyyy");
                    var isCurrentYear = year.ToString("yyyy").Equals(DateTime.UtcNow.ToString("yyyy"));
                    // to calculate the current mnonth range
                    // count from 1 to the current month for the current year
                    // count from 1 to 12 for other years - note the count-from specifics
                    int maximalMonthInYear = isCurrentYear ? Convert.ToInt32(DateTime.UtcNow.ToString("MM")) :
                                    13; // Convert.ToInt32(DateTime.Now.ToString("MM"));
                    for (int counterMonth = 1; counterMonth < (maximalMonthInYear - 1); counterMonth++)
                    {
                        foreach (var aggregationPrefix in aggregationPrefixes)
                        {
                            string tableNameStem = policy.WADMetricsTableNamePrefix +
                                               aggregationPrefix +
                                               "P10DV2S" + counterYear + counterMonth.ToString("d2"); //convert single digit month to two digit


                            string tableName = string.Empty;

                            // note this line participates in count-from issues for this module


                            try
                            {
                                try
                                {
                                    foreach (var day in this.AllDatesInMonth(int.Parse(counterYear), counterMonth))
                                    {
                                        tableName = tableNameStem + day.ToString("d2");

                                        try
                                        {
                                            // var allTablesResult = await cloudTableClient.ListTablesSegmentedAsync(tableName, token);
                                            // tableSegmentResult = allTablesResult;
                                            // token = allTablesResult.ContinuationToken;
                                            if (wadMetricsTables.Contains(tableName))
                                            {
                                                ret.MetricsRetentionSurfaceItemEntities.Add(new MetricsRetentionSurfaceItemEntity()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage"),
                                                    ItemDescription = "WADMetrics: Standard prefix for all WADMetrics tables",
                                                    ItemExists = true,
                                                    ItemType = RetentionSurfaceItemDescriptor.IsMetricsTableItem,
                                                    ItemWillBeDeleted = true,
                                                    PolicyAgeTriggerInMonths = policy.DeleteOlderTablesThanCurrentMonthMinusThis,
                                                    StorageAccountId = storageAccount.Id,
                                                    SuscriptionId = storageAccount.SubscriptionId,
                                                    TableName = tableName
                                                });

                                                log.LogInformation(tableName + " found");
                                            }
                                            else
                                            {

                                                log.LogTrace($"did not find table {tableName}");
                                                ret.MetricsRetentionSurfaceItemEntities.Add(new MetricsRetentionSurfaceItemEntity()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    DocumentationLink = new Uri("https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-template#wadmetrics-tables-in-storage"),
                                                    ItemDescription = "WADMetrics: Standard prefix for all WADMetrics tables",
                                                    ItemExists = false,
                                                    ItemType = RetentionSurfaceItemDescriptor.IsMetricsTableItem,
                                                    ItemWillBeDeleted = true,
                                                    PolicyAgeTriggerInMonths = policy.DeleteOlderTablesThanCurrentMonthMinusThis,
                                                    StorageAccountId = storageAccount.Id,
                                                    SuscriptionId = storageAccount.SubscriptionId,
                                                    TableName = tableName
                                                });

                                                log.LogTrace(tableName + "not found");

                                            }
                                        }
                                        catch (Exception e)
                                        {

                                            log.LogTrace($"exception testing table names {e.Message}");
                                        }
                                    }

                                }
                                catch (Exception e)
                                {

                                    log.LogTrace($"exception testing table names {e.Message}");
                                    // ret.Add(tableName + " not found");

                                }

                            }
                            catch (Exception e)
                            {
                                log.LogTrace($"exception testing table names {e.Message}");

                            }

                        }
                    }
                }


                //} while (token != null);
                Console.WriteLine("Old Tables audited");
            }
            catch (Exception ex)
            {
                log.LogError("Exception occured while auditing tables " + ex.Message);
            }

            var returnedItems = ret.MetricsRetentionSurfaceItemEntities.Count();
            return ret;
        }

        /// <summary>
        /// audit WAD & LAD et all diagnostic log surface
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="cloudTableClient"></param>
        /// <returns></returns>
        public async Task<DiagnosticsRetentionSurfaceEntity> AuditDiagnosticsRetentionSurface(string authToken, StorageAccountEntity storageAccount, TableStorageEntityRetentionPolicyEntity policy)
        {
            TableContinuationToken token = null;

            if (authToken == null || storageAccount == null || policy == null)
            {
                return new DiagnosticsRetentionSurfaceEntity();
            }

            var storageCreds = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.Key);
            var cloudTableClient = new CloudTableClient(storageAccount.PrimaryTableStorageEndpoint, storageCreds);

            var ret = policy.DiagnosticsRetentionSurface;
            try
            {
                foreach (var table in policy.DiagnosticsRetentionSurface.DiagnosticsRetentionSurfaceEntities)
                {
                    try
                    {

                        var allTablesResult = await cloudTableClient.ListTablesSegmentedAsync(table.TableName, token);
                        try
                        {
                            if (allTablesResult != null && allTablesResult.Count() > 0)
                            {
                                ret.DiagnosticsRetentionSurfaceEntities.Find(w => w.Id.Equals(table.Id)).ItemExists = true;
                            }
                            else
                            {
                                ret.DiagnosticsRetentionSurfaceEntities.Find(w => w.Id.Equals(table.Id)).ItemExists = false;

                            }
                        }
                        catch (Exception e)
                        {

                            log.LogError($"exception auditing diagnostics retention surface {e.Message}");
                        }

                    }
                    catch (Exception e)
                    {

                        log.LogWarning($"error auditing diagnostics retention surface: table name {table.TableName} caused error -> {e.Message}");
                    }
                }

                log.LogInformation($"found { ret.DiagnosticsRetentionSurfaceEntities.Where(w => w.ItemExists == true).Count()} diagnostics tables in storage account");
            }
            catch (Exception e)
            {
                log.LogError($"problem finding diagnostics in storage account {e.Message}");
            }

            return ret;
        }

        /// <summary>
        /// find all tables in storage account that begin with WADMetrics
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="cloudTableClient"></param>
        /// <returns></returns>
        private async Task<List<string>> GetWadMetricsTablesInStorageAccount(TableStorageTableRetentionPolicyEntity policy, CloudTableClient cloudTableClient)
        {
            TableContinuationToken token = null;
            var wadMetricsTables = new List<string>();
            try
            {
     
                var allTablesResult = await cloudTableClient.ListTablesSegmentedAsync(policy.WADMetricsTableNamePrefix, token);
                if (allTablesResult != null && allTablesResult.Count() > 0)
                {

                    wadMetricsTables.AddRange(allTablesResult.Results.Select(s => s.Name).ToList());
                }
                while (allTablesResult.ContinuationToken != null)
                {
                    wadMetricsTables.AddRange(allTablesResult.Results.Select(s => s.Name).ToList());
                }

                log.LogInformation($"found WADMetrics {allTablesResult.Count()} tables in storage account");
            }
            catch (Exception e)
            {
                log.LogError($"problem finding WADMetrics tables in storage account {e.Message}");
            }

            return wadMetricsTables;
        }

        /// <summary>
        /// this is unused but clever so ima leave it right here
        /// as per https://stackoverflow.com/questions/9097027/foreach-day-in-month
        /// foreach(day in month of year)
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [Obsolete]
        private IEnumerable<int> AllDatesInMonth(int year, int month)
        {
            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                yield return new DateTime(year, month, day).Day;
            }
        }

        /// <summary>
        /// delete old metrics table both PT1H and PT1M
        /// </summary>
        /// <param name="tablepPrefix">WADMetricsPT1HP10DV2S and WADMetricsPT1MP10DV2S</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<List<String>> DeleteOldTables(string authToken, StorageAccountEntity storageAccount, TableStorageTableRetentionPolicyEntity policy)
        {
            // as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
            string[] prefixes = { "PT1H", "PT1M" };
            var ret = new List<String>();
            try
            {

                var storageCreds = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccount.Name, storageAccount.Key);
                var cloudTableClient = new CloudTableClient(storageAccount.PrimaryTableStorageEndpoint, storageCreds);

                // 40 mths = 40/12 TODO validate rounding error captures remainder in months
                var yearCount = policy.DeleteOlderTablesThanCurrentMonthMinusThis / 12;


                TableContinuationToken token = null;
                List<DateTime> years = new List<DateTime>();
                for (var count = yearCount; count >= 0; count--)
                {
                    // subtract x number of years from now
                    var year = DateTime.Now.Subtract(new TimeSpan(365 * count, 0, 0, 0));
                    years.Add(year);
                }

                do
                {
                    // as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
                    foreach (string tablepPrefix in prefixes)
                    {
                        foreach (var year in years)
                        {
                            string currentYear = year.ToString("yyyy"); // DateTime.Now.ToString("yyyy");
                            int currentMonth = Convert.ToInt32(year.ToString("MM")); // Convert.ToInt32(DateTime.Now.ToString("MM"));
                            for (int i = 1; i < (currentMonth - 1); i++)
                            {
                                string currentTablePrefix = tablepPrefix + currentYear + i.ToString("d2"); //convert single digit month to two digit
                                var allTablesResult = await cloudTableClient.ListTablesSegmentedAsync(currentTablePrefix, token);
                                token = allTablesResult.ContinuationToken;

                                Console.WriteLine("Fetched all tables to be deleted, count: " + allTablesResult.Results.Count);

                                foreach (CloudTable table in allTablesResult.Results)
                                {
                                    if (policy.PolicyEnforcementMode == PolicyEnforcementMode.ApplyPolicy)
                                    {
                                        log.LogInformation("Deleting table: " + table.Name);
                                        ret.Add(table.Name);
                                        await table.DeleteIfExistsAsync();

                                    }
                                    else
                                    {
                                        log.LogInformation("would have deleted table. running in whatif mode");
                                        ret.Add(table.Name);
                                    }
                                }
                            }
                        }
                    }

                } while (token != null);
                Console.WriteLine("Old Tables Deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while deleting tables " + ex.Message);
            }

            return ret;
        }

        /// <summary>
        /// as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="cloudTable"></param>
        /// <returns></returns>
        public async Task<int> DeleteOldDiagnosticsEntitiesForTable(String ticks, CloudTable cloudTable)
        {

            // this query may take a while
            TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity> query = new TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity>() { TakeCount = 1000 }
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, ticks))
                .Select(new List<string>() { "PartitionKey", "RowKey" });

            // Console.WriteLine("Deleting data from " + cloudTable.Name + " using query " + query.FilterString);

            TableContinuationToken token = null;
            int ret = 0;

            try
            {
                do
                {

                    Debug.WriteLine("Fetching Records");
                    TableQuerySegment<Microsoft.WindowsAzure.Storage.Table.TableEntity> resultSegment = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
                    token = resultSegment.ContinuationToken;

                    // update the count of records fetched
                    ret += resultSegment.Count();

                    log.LogTrace("Fetched all records to be deleted, count: " + resultSegment.Results.Count);

                    foreach (Microsoft.WindowsAzure.Storage.Table.TableEntity entity in resultSegment.Results)
                    {
                        log.LogInformation("Deleting entry with TimeStamp: " + entity.Timestamp.ToString());
                        TableOperation deleteOperation = TableOperation.Delete(entity);
                        await cloudTable.ExecuteAsync(deleteOperation);
                    }
                } while (token != null);

                log.LogInformation(String.Format("policy applied to {0} entities in tabled named {1}", ret, cloudTable.Name));

                return ret;
            }
            catch (Exception ex)
            {
                log.LogError("Exception occured while deleting data from table " + cloudTable.Name + " " + ex.Message);
            }

            return await Task.FromResult<int>(ret);
        }
        public async Task<int> AuditOldDiagnosticsEntitiesForTable(String ticks, CloudTable cloudTable)
        {

            // this query may take a while
            TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity> query = new TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity>() { TakeCount = 1000 }
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, ticks))
                .Select(new List<string>() { "PartitionKey", "RowKey" });

            // Console.WriteLine("Deleting data from " + cloudTable.Name + " using query " + query.FilterString);

            TableContinuationToken token = null;
            int ret = 0;

            try
            {
                do
                {

                    Debug.WriteLine("Fetching Records");
                    TableQuerySegment<Microsoft.WindowsAzure.Storage.Table.TableEntity> resultSegment = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
                    token = resultSegment.ContinuationToken;

                    // update the count of records fetched
                    ret += resultSegment.Count();

                    log.LogTrace("Fetched all records to be deleted, count: " + resultSegment.Results.Count);

                } while (token != null);

                log.LogInformation(String.Format("policy applied to {0} entities in tabled named {1}", ret, cloudTable.Name));

                return ret;
            }
            catch (Exception ex)
            {
                log.LogError("Exception occured while deleting data from table " + cloudTable.Name + " " + ex.Message);
            }

            return await Task.FromResult<int>(ret);
        }

    }
}
