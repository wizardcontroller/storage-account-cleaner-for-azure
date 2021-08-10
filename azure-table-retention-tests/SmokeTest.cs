using AutoMapper;
using Azure.Identity;
using com.ataxlab.azure.table.retention.models;
using com.ataxlab.azure.table.retention.models.automapper;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.functions.table.retention.parameters;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention
{
    public class Tests
    {
        public IConfiguration LocalSettings;


        ITableEntityRetentionClient client = null;

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();
            return config;
        }

        // control this test with these variables
        int somereasonablenumber = 100;

        // parts of the test won't execute if these numbers 
        // are higher than this.somereasonablenumber
        // to keep storage transactions low during testing
        int tableDeletionAgeInMonths = 999;
        int entityDeletionAgeInDays = 999;


        [SetUp]
        public void Setup()
        {
            LocalSettings = InitConfiguration();

            var clientSecret = LocalSettings["values:ServicePrincipalPassword"];
            var clientId = LocalSettings["values:ServicePrincipalId"];
            var tenantId = LocalSettings["values:TenantId"];
            var telemetryKey = LocalSettings["values:ApplicationInsightsKey"];
            var subscriptionId = LocalSettings["values:SubscriptionKey"];


            var telemmetryConfig = new TelemetryConfiguration(telemetryKey);
            var telemmetryClient = new TelemetryClient(telemmetryConfig);

            // in testing we use the client credentials flow
            // the deployment environment is azure functions
            // therefore a different TableEntityRetentionClient ctor
            // is 'used' by dependency injection to supply
            // a Functions Runtime initialized TelemetryConfiguration
            // and the Function App's Managed Identity supplies 
            // the AzureCredentials
            var cred = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud)
                       .WithDefaultSubscription(subscriptionId);

            // becaues we are using automapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
                //cfg.CreateMap<EndpointsDto, Endpoints>();
                //cfg.CreateMap<PublicEndpointsDto, PublicEndpoints>();
            });

            config.AssertConfigurationIsValid();
            client = new TableEntityRetentionClient(cred, telemmetryConfig, config.CreateMapper());

        }

        [Test]
        public void RenderApplianceCommands()
        {
            var cmd = new WorkflowOperationCommand()
            {
                CommandCode = WorkflowOperation.GetCurrentWorkflowCheckpoint,
                TimeStamp = DateTime.UtcNow
            };

            var json = JsonConvert.SerializeObject(cmd);
            Debug.WriteLine ("GetCurrentWorkflowCheckpoint");
            Debug.WriteLine(json);


            cmd = new WorkflowOperationCommand()
            {
                CommandCode = WorkflowOperation.ApplyEnvironmentRetentionPolicy,
                TimeStamp = DateTime.UtcNow
            };

            json = JsonConvert.SerializeObject(cmd);
            Debug.WriteLine("ApplyEnvironmentRetentionPolicy");
            Debug.WriteLine(json);

            cmd = new WorkflowOperationCommand()
            {
                CommandCode = WorkflowOperation.BeginWorkflow,
                TimeStamp = DateTime.UtcNow
            };

            json = JsonConvert.SerializeObject(cmd);
            Debug.WriteLine("BeginEnvironmentDiscovery");
            Debug.WriteLine(json);

            cmd = new WorkflowOperationCommand()
            {
                CommandCode = WorkflowOperation.CommitRetentionPolicyConfiguration,
                TimeStamp = DateTime.UtcNow
            };

            json = JsonConvert.SerializeObject(cmd);
            Debug.WriteLine("CommitRetentionPolicyConfiguration");
            Debug.WriteLine(json);

            cmd = new WorkflowOperationCommand()
            {
                CommandCode = WorkflowOperation.ProvisionAppliance,
                TimeStamp = DateTime.UtcNow
            };

            json = JsonConvert.SerializeObject(cmd);
            Debug.WriteLine("ProvisionAppliance");
            Debug.WriteLine(json);
            Assert.IsTrue(json != null,"failed to render Appliance Commands");
        }
        
        [Test]
        public void RenderOrchestratorStarterCommands()
        {
            ApplyWADDDefaultPolicyParameter defaultParam = new ApplyWADDDefaultPolicyParameter()
            {
                EntityRetentionAgeInDays = this.entityDeletionAgeInDays,
                PolicyEnforcementMode = policyEnforcementMode.WhatIf,
                
            };
        }

        [Test]
        public async Task Test1()
        {

            try
            {
 
                // exercise the ability of the client to 
                // use the credentials supplied and RBAC
                // backing the credentials to enumerate 
                // storage accounts in a subscription
                var v2StorageAccounts = await client.GetAllV2StorageAccounts();

                Assert.IsTrue(v2StorageAccounts.Count > 0, "failed to retrieve storage accounts");


                // we need only work with StorageV2
                // because those have Table storage
                // CloudTable cloudTable = cloudTableClient.GetTableReference("WADPerformanceCountersTable"); //do this for all other logs table too
                List<string> tableNames = await ApplyPoliciesToStorageAccounts(client, v2StorageAccounts);

                // tune these variables to get policies that will actually apply to something
                if (this.entityDeletionAgeInDays < somereasonablenumber && this.tableDeletionAgeInMonths < somereasonablenumber)
                {
                    Assert.IsTrue(tableNames.Count() > 0, "test environment does not contain any table storage with tables");
                }

                Assert.IsNotNull(v2StorageAccounts);
            }
            catch (Exception ex)
            {

                Assert.Fail();
            }
        }

        private async Task<List<string>> ApplyPoliciesToStorageAccounts(ITableEntityRetentionClient client, List<StorageAccountModel> v2StorageAccounts)
        {
            List<String> tableNames = new List<String>();

            // source of tuth for providing psudo-hardcoded names
            // we want to match against in this test
            var defaultRetentionConfiguration = new DefaultTableRetentionConfiguration();

            foreach (var act in v2StorageAccounts)
            {
                if (act.Kind == Microsoft.Azure.Management.Storage.Fluent.Models.Kind.StorageV2)
                {
                    // exercise the ability to retrieve storage keys 
                    // for a storage account and use the service key
                    // to enumerate the table names in Table storage
                    tableNames = await client.GetTableNamesInStorageAccount(act);

                    List<string> matchedDiagnosticsTables = client.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADDIagnosticsTableNameMatchPatterns);
                    // List<string> matchedMetricsTables = client.GetCandidateMatches(tableNames, defaultRetentionConfiguration.WADMetricsTableNameMatchPatterns);
                    List<string> matchedMetricsTables = tableNames.Where(w => w.StartsWith("WADMetrics")).ToList();
                    // audit metric entities
                    var entities = await client.GetMetricEntitiesForTableNames(act, matchedMetricsTables);

                    var tableRetentionPolicy = client.BuildTableStorageTableRetentionPolicy(tableDeletionAgeInMonths, matchedMetricsTables);
                    var entityRetentionPolicy = client.BuildTableStorageEntityRetentionPolicy(entityDeletionAgeInDays, matchedDiagnosticsTables);
                    var retentionPolicy = new TableStorageRetentionPolicy()
                    {
                        TableStorageEntityRetentionPolicy = entityRetentionPolicy,
                        TableStorageTableRetentionPolicy = tableRetentionPolicy
                    };



                    var storageTableClient = client.GetStorageTableClient(act);


                    var storageTable = client.GetStorageTableReference(storageTableClient, tableRetentionPolicy.TableNames[0]);


                    var result = await client.ApplyTableStorageEntityRetentionPolicy(act, entityRetentionPolicy,  () => { return "0" + DateTime.UtcNow.AddDays(-entityRetentionPolicy.NumberOfDays).Ticks.ToString(); });
                    // here because we can connect to a storage account find some table names we're interested in
                    // and get a table reference to one of them

                    // remove these post extract method
                    if (tableDeletionAgeInMonths == 999) ;
                    if (entityDeletionAgeInDays == 999) ;
                    Assert.IsTrue(matchedDiagnosticsTables.Count > 0);

                }
            }

            return tableNames;
        }
    }
}