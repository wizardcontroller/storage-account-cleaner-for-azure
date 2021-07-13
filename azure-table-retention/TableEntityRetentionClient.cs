using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Identity;
using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.state.entities;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Management.Cdn.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Azure.Management.Storage.Fluent.StorageAccount.Definition;
using Microsoft.Azure.Storage.Auth;
using Microsoft.IdentityModel.Protocols;
using Microsoft.OData.UriParser;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace com.ataxlab.functions.table.retention
{
    public interface ITableEntityRetentionClient
    {
        Task<List<string>> GetTableNames(TableServiceClient tableClient);
        Task<List<string>> GetTableNamesInStorageAccount(StorageAccountModel account);
        Task<List<StorageAccountModel>> GetAllStorageAccounts(bool loadAllPages = true);

        /// <summary>
        /// as per https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="cloudTable"></param>
        /// <returns></returns>
        Task<int> DeleteOldData(string ticks, CloudTable cloudTable);
        TableStorageEntityRetentionPolicy BuildTableStorageEntityRetentionPolicy(int tableEntityRetentionInDays, List<string> candidateTableNames);
        TableStorageTableRetentionPolicy BuildTableStorageTableRetentionPolicy(int tableDeletionAgeInMonths, List<string> candidateTableNames);
        Task<TableStorageEntityRetentionPolicyEnforcementResult> ApplyTableStorageEntityRetentionPolicy(StorageAccountModel acct, TableStorageEntityRetentionPolicy policy, Func<string> tickProvider);
        TableStorageTableRetentionPolicyEnforcementResult ApplyPolicy(StorageAccountModel acct, TableStorageTableRetentionPolicy policy);
        List<string> GetCandidateMatches(List<string> tableNames, List<string> defaultTableMatchPatterns);
        CloudTable GetStorageTableReference(CloudTableClient client, string tableName);
        CloudTableClient GetStorageTableClient(StorageAccountModel account);
        CloudTableClient GetCloudTableClient(string connectionString);
        string GetStorageAccountKey(StorageAccountModel account);
        Uri GetStorageAccountPrimaryEndpoint(StorageAccountModel account);
        string GetStorageAccountName(StorageAccountModel account);
        string GetStorageAccountConnectionString(string key, string accountName);
        string GetTableStorageAccountConnectionString(string key, string accountName);
        TableServiceClient GetTableServiceClient(StorageAccountModel account);
        Task<List<StorageAccountModel>> GetAllV2StorageAccounts();
        Task<List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>> GetRetentionPolicyTuples(List<StorageAccountModel> v2StorageAccounts);
        Task<Tuple<TableStorageRetentionPolicy, StorageAccountModel>> GetDefaultTableStorageRetentionPolicy(StorageAccountModel storageAccount);
        Task<List<DiagnosticsRetentionSurfaceItemEntity>> GetMetricEntitiesForTableNames(StorageAccountModel account, List<string> tableNames);
    }

    /// <summary>
    /// as per https://www.koskila.net/how-to-access-azure-function-apps-settings-from-c/
    /// as per https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/tables/Azure.Data.Tables/samples/Sample0Auth.md
    /// as per https://docs.microsoft.com/en-us/samples/azure-samples/storage-dotnet-resource-provider-getting-started/storage-dotnet-resource-provider-getting-started/
    /// </summary>
    public class TableEntityRetentionClient : ITableEntityRetentionClient
    {
        private IAzure iAzure;
        private AzureCredentials azureCredentials;
        private IMapper autoMapper;
        private TelemetryClient telemetryClient;

        /// <summary>
        /// as per https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=v2%2Ccmd#log-custom-telemetry-in-c-functions
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="appInsightsClient"></param>
        [Obsolete]
        private TableEntityRetentionClient(AzureCredentials creds, TelemetryClient appInsightsClient)
        {

            azureCredentials = creds;
            telemetryClient = appInsightsClient;

            telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:Ctor:WithTelemetryConfiguration:WillInitIAzure"));

            InitIAzure(creds);
        }

        /// <summary>
        /// extra
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="telemetryConfiguration"></param>
        public TableEntityRetentionClient(AzureCredentials creds, TelemetryConfiguration telemetryConfiguration, IMapper mapper)
        {

            azureCredentials = creds;
            this.autoMapper = mapper;
            if (telemetryConfiguration != null) telemetryClient = new TelemetryClient(telemetryConfiguration);
            telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:Ctor:WithTelemetryConfiguration:WillInitIAzure"));

            InitIAzure(creds);
        }

        /// <summary>
        /// for testing
        /// </summary>
        /// <param name="telemetryKey"></param>
        public TableEntityRetentionClient(AzureCredentials creds, string telemetryKey = null)
        {
            azureCredentials = creds;

            if (telemetryKey != null)
            {
                TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(telemetryKey);
                telemetryClient = new TelemetryClient(telemetryConfiguration);
            }

            telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:Ctor:WithTelemetryKey:WillInitIAzure"));
            InitIAzure(creds);

        }

        private void InitIAzure(AzureCredentials creds)
        {
            IAzure authResult = null;
            try
            {
                telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:InitIAzure:WillAuthenticate"));
                authResult = Authenticate(creds);
                iAzure = authResult;
                telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:InitIAzure:DidAuthenticate"));
            }
            catch (Exception e)
            {
                telemetryClient?.TrackEvent(new EventTelemetry("TableEntityRetentionClient:InitIAzure:FailedAuthenticate"));
            }

        }


        /// <summary>
        /// as per https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md
        /// </summary>
        /// <returns></returns>
        private IAzure Authenticate(AzureCredentials creds)
        {
            // as per https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/identity/Azure.Identity#credential-classes
            // as per https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/tables/Azure.Data.Tables/samples/Sample0Auth.md

            return Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(creds)
                .WithDefaultSubscription();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v2StorageAccounts"></param>
        /// <returns></returns>
        public async Task<List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>> GetRetentionPolicyTuples(List<StorageAccountModel> v2StorageAccounts)
        {
            var ret = new List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>();

            foreach (var storageAccount in v2StorageAccounts)
            {
                Tuple<TableStorageRetentionPolicy, StorageAccountModel> newTuple = await GetDefaultTableStorageRetentionPolicy(storageAccount);

                ret.Add(newTuple);
            }

            Debug.WriteLine(string.Format("Got All V2 Storage Accounts"));
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageAccount"></param>
        /// <returns></returns>
        public async Task<Tuple<TableStorageRetentionPolicy, StorageAccountModel>> GetDefaultTableStorageRetentionPolicy(StorageAccountModel storageAccount)
        {
            var tableNames = await this.GetTableNamesInStorageAccount(storageAccount);
            var defaultRetentionConfiguration = new DefaultTableRetentionConfiguration();
            List<string> matchedDiagnosticsTables = this.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADDIagnosticsTableNameMatchPatterns);
            List<string> matchedMetricsTables = this.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADMetricsTableNameMatchPatterns);
            int tableDeletionAgeInMonths = 9;
            int entityDeletionAgeInDays = 9;

            var tableRetentionPolicy = this.BuildTableStorageTableRetentionPolicy(tableDeletionAgeInMonths, matchedMetricsTables);
            var entityRetentionPolicy = this.BuildTableStorageEntityRetentionPolicy(entityDeletionAgeInDays, matchedDiagnosticsTables);
            var retentionPolicy = new TableStorageRetentionPolicy()
            {
                TableStorageEntityRetentionPolicy = entityRetentionPolicy,
                TableStorageTableRetentionPolicy = tableRetentionPolicy
            };

            var newTuple = new Tuple<TableStorageRetentionPolicy, StorageAccountModel>(retentionPolicy, storageAccount);
            return newTuple;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<StorageAccountModel>> GetAllV2StorageAccounts()
        {
            var allAccounts = await GetAllStorageAccounts();
            var result = allAccounts.Where(w => w.Kind == Kind.StorageV2).ToList<StorageAccountModel>();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<StorageAccountModel>> GetAllStorageAccounts(bool loadAllPages = true)
        {
            IPagedCollection<IStorageAccount> accounts = null;
            List<StorageAccountModel> ret = new List<StorageAccountModel>();

            try
            {
                telemetryClient.TrackEvent("TableEntityRetentionClient:StorageAccounts:WillListedAll");

                accounts = await iAzure.StorageAccounts.ListAsync(loadAllPages);

                var iterator = accounts.GetEnumerator();
                while (iterator.MoveNext())
                {
                    // cast the upstream object to the dto
                    PublicEndpoints endpoints = iterator.Current.EndPoints;

                    PublicEndpointsDto dto = this.autoMapper.Map<PublicEndpoints, PublicEndpointsDto>(endpoints);
                    // var reslt = autoMapper.Map(endpoints, nameof(PublicEndpoints),nameof(PublicEndpoints))
                    ret.Add(new StorageAccountModel()
                    {
                        EndPoints = dto,
                        Keys = await iterator.Current.GetKeysAsync(),
                        Kind = iterator.Current.Kind,
                        Name = iterator.Current.Name
                    });
                }

                telemetryClient.TrackEvent("TableEntityRetentionClient:StorageAccounts:ListedAll");
            }
            catch (Exception e)
            {
                telemetryClient?.TrackException(e);
            }

            return ret;
        }

        /// <summary>
        /// as per https://github.com/Azure/azure-sdk-for-net/blob/Azure.Data.Tables_3.0.0-beta.5/sdk/tables/Azure.Data.Tables/README.md
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private async Task<List<string>> GetTableNamesInStorageAccount(IStorageAccount account)
        {
            TableServiceClient tableClient = GetTableServiceClient(account);

            List<string> existingTableNames = await GetTableNames(tableClient);

            return existingTableNames;
        }

        /// <summary>
        /// as per https://github.com/Azure/azure-sdk-for-net/blob/Azure.Data.Tables_3.0.0-beta.5/sdk/tables/Azure.Data.Tables/README.md
        /// do not remove - this is on the test code path
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task<List<string>> GetTableNamesInStorageAccount(StorageAccountModel account)
        {
            List<string> existingTableNames = new List<string>();

            try
            {


                TableServiceClient tableClient = GetTableServiceClient(account);
                //var tokenCredentials = new DefaultAzureCredential(includeInteractiveCredentials: true);
                //var impersonate = await tokenCredentials.GetTokenAsync(new TokenRequestContext(new string[] { "https://management.azure.com/user_impersonation" })).ConfigureAwait(false);
                //var impersonate2 = await tokenCredentials.GetTokenAsync(new TokenRequestContext(new string[] { account.EndPoints.Primary.Table + ".default" })).ConfigureAwait(false);

                //var token = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(impersonate2.Token);
                //// get a cloud table client for this storage account
                //var cloudTableClient = new CloudTableClient(new Uri(account.EndPoints.Primary.Table.), token);


                var names= await GetTableNames(tableClient);

                existingTableNames = names;
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

            return existingTableNames;
        }

        /// <summary>
        /// testing codepath
        /// proof of concept - read low watermark and high watermark
        /// entity timestamp in storage account table
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        public async Task<List<DiagnosticsRetentionSurfaceItemEntity>> GetMetricEntitiesForTableNames(StorageAccountModel account, List<string> tableNames)
        {
            var existingTableNames = new List<DiagnosticsRetentionSurfaceItemEntity>();

            var ticks = "0" + DateTime.UtcNow.AddYears(-5).Ticks;
            try
            {
                TableContinuationToken token = null;
                foreach (var tableName in tableNames)
                {

                    TableServiceClient tableClient = GetTableServiceClient(account);
                    TableClient table = tableClient.GetTableClient(tableName);
                    TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity> query = new TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity>() { TakeCount = 1 }
                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, ticks))
                        .Select(new List<string>() { "PartitionKey", "RowKey" });

                    var resultSegment = table.QueryAsync<Azure.Data.Tables.TableEntity>(q => 

                                                                                        (q.Timestamp < DateTime.UtcNow.AddMinutes(-25))
                                                                                        
                                                                                        );
                    var res = resultSegment.AsPages();
                    var enumerator = resultSegment.GetAsyncEnumerator();
                    var r = await enumerator.MoveNextAsync();
                    do
                    {
                        var a = enumerator.Current;
                        r = false;
                    }
                    while (r == true);

                    var rx = 'x';
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

            return existingTableNames;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public TableServiceClient GetTableServiceClient(StorageAccountModel account)
        {
            string key = GetStorageAccountKey(account);
            Uri primaryEndpoint = GetStorageAccountPrimaryEndpoint(account);
            string accountName = GetStorageAccountName(account);
            string connectionString = GetStorageAccountConnectionString(key, accountName);

            CloudTableClient cloudTableClient = GetCloudTableClient(connectionString);

            var x = cloudTableClient.BaseUri;

            var tableClient = new TableServiceClient(primaryEndpoint, new TableSharedKeyCredential(accountName, key));
            return tableClient;
        }

        /// <summary>
        /// needed by Azure SDK
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        [Obsolete]
        private TableServiceClient GetTableServiceClient(IStorageAccount account)
        {
            string key = GetStorageAccountKey(account);
            Uri primaryEndpoint = GetStorageAccountPrimaryEndpoint(account);
            string accountName = GetStorageAccountName(account);
            string connectionString = GetStorageAccountConnectionString(key, accountName);

            CloudTableClient cloudTableClient = GetCloudTableClient(connectionString);

            var x = cloudTableClient.BaseUri;

            var tableClient = new TableServiceClient(primaryEndpoint, new TableSharedKeyCredential(accountName, key));
            return tableClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public string GetTableStorageAccountConnectionString(string key, string accountName)
        {
            // var result = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={0}.table.core.windows.net", accountName, key);
            var result = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net", accountName, key);
            int i = 0;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public string GetStorageAccountConnectionString(string key, string accountName)
        {
            var result = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net", accountName, key);
            int i = 0;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public string GetStorageAccountName(StorageAccountModel account)
        {
            return account.Name;
        }

        [Obsolete]
        private string GetStorageAccountName(IStorageAccount account)
        {
            return account.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public Uri GetStorageAccountPrimaryEndpoint(StorageAccountModel account)
        {
            return new Uri(account.EndPoints.Primary.Table);
        }


        [Obsolete]
        private Uri GetStorageAccountPrimaryEndpoint(IStorageAccount account)
        {
            return new Uri(account.EndPoints.Primary.Table);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public string GetStorageAccountKey(StorageAccountModel account)
        {
            return account.Keys.First().Value;
        }


        [Obsolete]
        private string GetStorageAccountKey(IStorageAccount account)
        {
            return account.GetKeys().First().Value;
        }

        /// <summary>
        /// needed by Cloud Storage SDK
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        [Obsolete]
        public CloudTableClient GetCloudTableClient(string connectionString)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            return cloudTableClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public CloudTableClient GetStorageTableClient(StorageAccountModel account)
        {
            string key = GetStorageAccountKey(account);
            string accountName = GetStorageAccountName(account);
            string connectionString = GetTableStorageAccountConnectionString(key, accountName);

            return GetCloudTableClient(connectionString);
        }

        [Obsolete]
        private CloudTableClient GetStorageTableClient(IStorageAccount account)
        {
            string key = GetStorageAccountKey(account);
            string accountName = GetStorageAccountName(account);
            string connectionString = GetTableStorageAccountConnectionString(key, accountName);

            return GetCloudTableClient(connectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public CloudTable GetStorageTableReference(CloudTableClient client, String tableName)
        {
            CloudTable cloudTable = client.GetTableReference(tableName);
            return cloudTable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableClient"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableNames"></param>
        /// <param name="defaultTableMatchPatterns"></param>
        /// <returns></returns>
        public List<string> GetCandidateMatches(List<string> tableNames, List<string> defaultTableMatchPatterns)
        {
            List<String> matchedTables = new List<string>();
            // identify WAD tables in the tables in this storage acount
            // i admit it - i tried an failed to express this in join notation in less than 10 seconds 
            matchedTables.AddRange(tableNames.Where(w => defaultTableMatchPatterns.Any(a => w.Contains(a))).ToList());
            return matchedTables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acct"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public TableStorageTableRetentionPolicyEnforcementResult ApplyPolicy(StorageAccountModel acct, TableStorageTableRetentionPolicy policy)
        {
            var ret = new TableStorageTableRetentionPolicyEnforcementResult() { Policy = policy };
            var storageTableClient = this.GetStorageTableClient(acct);

            foreach (var tableName in policy.TableNames)
            {
                var storageTable = this.GetStorageTableReference(storageTableClient, tableName);

            }

            return ret;
        }

        [Obsolete]
        private TableStorageTableRetentionPolicyEnforcementResult ApplyPolicy(IStorageAccount acct, TableStorageTableRetentionPolicy policy)
        {
            var ret = new TableStorageTableRetentionPolicyEnforcementResult() { Policy = policy };
            var storageTableClient = this.GetStorageTableClient(acct);

            foreach (var tableName in policy.TableNames)
            {
                var storageTable = this.GetStorageTableReference(storageTableClient, tableName);

            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acct"></param>
        /// <param name="policy"></param>
        /// <param name="tickProvider"></param>
        /// <returns></returns>
        public async Task<TableStorageEntityRetentionPolicyEnforcementResult> ApplyTableStorageEntityRetentionPolicy(StorageAccountModel acct, TableStorageEntityRetentionPolicy policy, Func<String> tickProvider)
        {
            var ret = new TableStorageEntityRetentionPolicyEnforcementResult() { Policy = policy };


            var storageTableClient = this.GetStorageTableClient(acct);

            try
            {
                foreach (var tableName in policy.TableNames)
                {
                    var storageTable = this.GetStorageTableReference(storageTableClient, tableName);
                    ret.PolicyTriggerCount += await this.DeleteOldData(policy.GetTicks(tickProvider), storageTable);

                    int i = 0;
                }
            }
            catch (Exception e)
            {
                int i = 0;
            }

            return ret;
        }


        /// <summary>
        /// supply your way or calculating date time in ticks
        /// </summary>
        /// <param name="acct"></param>
        /// <param name="policy"></param>
        /// <param name="tickProvider"></param>
        /// <returns></returns>
        [Obsolete]
        private async Task<TableStorageEntityRetentionPolicyEnforcementResult> ApplyPolicy(IStorageAccount acct, TableStorageEntityRetentionPolicy policy, Func<String> tickProvider = null)
        {
            var ret = new TableStorageEntityRetentionPolicyEnforcementResult() { Policy = policy };
            var storageTableClient = this.GetStorageTableClient(acct);

            try
            {
                foreach (var tableName in policy.TableNames)
                {
                    var storageTable = this.GetStorageTableReference(storageTableClient, tableName);
                    var result = await this.DeleteOldData(policy.GetTicks(tickProvider), storageTable);

                    int i = 0;
                }
            }
            catch (Exception e)
            {
                int i = 0;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableDeletionAgeInMonths"></param>
        /// <param name="candidateTableNames"></param>
        /// <returns></returns>
        public TableStorageTableRetentionPolicy BuildTableStorageTableRetentionPolicy(int tableDeletionAgeInMonths, List<String> candidateTableNames)
        {

            // render TableRetentionPolicy
            TableStorageTableRetentionPolicy tableRetentionPolicy = new TableStorageTableRetentionPolicy()
            {
                TableNames = candidateTableNames,
                DeleteOlderTablesThanCurrentMonthMinusThis = tableDeletionAgeInMonths
            };

            return tableRetentionPolicy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableEntityRetentionInDays"></param>
        /// <param name="candidateTableNames"></param>
        /// <returns></returns>
        public TableStorageEntityRetentionPolicy BuildTableStorageEntityRetentionPolicy(int tableEntityRetentionInDays, List<String> candidateTableNames)
        {
            TableStorageEntityRetentionPolicy policy = new TableStorageEntityRetentionPolicy()
            {
                NumberOfDays = tableEntityRetentionInDays,
                TableNames = candidateTableNames
            };

            return policy;
        }


        /// <summary>
        /// delete data from table older than ticks 
        /// beware that a durable function app has
        /// a special way of getting DateTime.Now
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="cloudTable"></param>
        /// <returns></returns>
        public async Task<int> DeleteOldData(String ticks, CloudTable cloudTable)
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

                    Debug.WriteLine("Fetched all records to be deleted, count: " + resultSegment.Results.Count);

                    int I = 0;
                    //foreach (Microsoft.WindowsAzure.Storage.Table.TableEntity entity in resultSegment.Results)
                    //{
                    //    Console.WriteLine("Deleting entry with TimeStamp: " + entity.Timestamp.ToString());
                    //    TableOperation deleteOperation = TableOperation.Delete(entity);
                    //    await cloudTable.ExecuteAsync(deleteOperation);
                    //}
                } while (token != null);

                Debug.WriteLine(String.Format("policy applied to {0} entities in tabled named {1}", ret, cloudTable.Name));

                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occured while deleting data from table " + cloudTable.Name + " " + ex.Message);
            }

            return ret;
        }
    }
}
