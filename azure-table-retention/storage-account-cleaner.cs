using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using com.ataxlab.functions.table.retention.parameters;
using com.ataxlab.functions.table.retention.utility;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;
using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.state.entities;
using DurableTask.Core.Stats;
using System.Threading;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using com.ataxlab.functions.table.retention.services;
using com.ataxlab.azure.table.retention.services.authorization;
using com.ataxlab.azure.table.retention.models;
using System.Linq;
using com.ataxlab.azure.table.retention.models.extensions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using com.ataxlab.functions.table.retention.entities;
using WorkflowOperation = com.ataxlab.functions.table.retention.entities.WorkflowOperation;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.Extensions.DiagnosticAdapter.Infrastructure;

namespace com.ataxlab.functions.table.retention
{
    public class AzureTableRetentionAppliance
    {
        ILogger<AzureTableRetentionAppliance> log;

        #region private orchestrator state of unserializables 
        private TelemetryClient TelemetryClient { get; set; }
        private ITableEntityRetentionClient TableEntityRetentionClient { get; set; }
        private ITableRetentionApplianceEngine TableRetentionApplianceEngine { get; set; }
        private ITableRetentionApplianceActivities TableRetentionApplianceActivities { get; set; }

        // private IDurableClient _iDurableClient;

        #endregion private orchestrator state of unserializables 
        //private IConfiguration _configuration;
        public AzureTableRetentionAppliance(TelemetryConfiguration telemetryConfiguration,
                                            //IConfiguration configuration,
                                            ITableEntityRetentionClient tableEntityRetentionClient,
                                           ITableRetentionApplianceEngine tableRetentionEngine,
                                            ILogger<AzureTableRetentionAppliance> classLogger)
        {


            log = classLogger;
            //_configuration = configuration;
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
            TableEntityRetentionClient = tableEntityRetentionClient;
            TableRetentionApplianceEngine = tableRetentionEngine;

            log.LogInformation("function dependencies injected");

            //var devEnvironment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT").ToLower().Contains("devel");
            //var hubnameSetting = System.Environment.GetEnvironmentVariable("ControlChannelConstants.TASKHUBNAME");
            //var hub = devEnvironment == true ? "TestHubName" : hubnameSetting;


        }

        //[FunctionName("azure_table_retention")]
        //public async Task DummyAzureTableRetention([OrchestrationTrigger] IDurableOrchestrationContext context,
        //                    [DurableClient] IDurableClient durableClient,
        //                                                           [DurableClient] IDurableEntityClient entityClient)
        //{
        //    var storageAccountOperation = context.WaitForExternalEvent<WorkflowOperationCommand>(ControlChannelConstants.WorkflowEvent_GetV2StorageAccounts);
        //    await Task.WhenAll(storageAccountOperation);
        //}

        [FunctionName("DiscoverEnvironmentWithDefaultWADiagnosticsPolicy")]
        public async Task RemoveThisAfterward([OrchestrationTrigger] IDurableOrchestrationContext context, [DurableClient] IDurableEntityClient entityClient,
                                                           [DurableClient] IDurableClient durableClient,
                                                           ILogger log)
        {
            log = context.CreateReplaySafeLogger(log);

            var tenantId = string.Empty;
            var subscriptionId = String.Empty;
            var oid = String.Empty;
            if (!context.IsReplaying)
            {
                var abort = await this.TableRetentionApplianceEngine.GetStateForUpdateWorkflowCheckpoints(entityClient, tenantId, subscriptionId, oid, WorkflowOperation.CancelWorkflow);

            }

        }

        [FunctionName(ControlChannelConstants.WorkflowEntrypointDebug)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Argument", "DurableFunctionOrchestrationTriggerAnalyzer:Orchestration Trigger Usage", Justification = "<Pending>")]
        public async Task DebugDefaultEnvronmentDiscoveryWorkflow([OrchestrationTrigger] IDurableOrchestrationContext context,
                                                           [DurableClient] IDurableEntityClient entityClient,
                                                           [DurableClient] IDurableClient durableClient,
                                                            ILogger log
                                                            )
        {
            log = context.CreateReplaySafeLogger(log);
            var tenantId = string.Empty;
            var subscriptionId = String.Empty;
            var oid = String.Empty;
            ApplianceSessionContextEntity appcontext;
            try
            {
                var applianceContext = context.GetInput<ApplianceSessionContextEntity>();
                appcontext = applianceContext;
                if (applianceContext != null)
                {
                    this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "workflow started",
                        detail = "workflow has retrieved appliance session context.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime
                    },
                tenantId, oid, entityClient).GetAwaiter().GetResult();

                    tenantId = applianceContext.TenantId;
                    subscriptionId = applianceContext.SelectedSubscriptionId;
                    oid = applianceContext.UserOid;

                    context.SetCustomStatus("started");
                }
            }
            catch (Exception e)
            {
                // tolerate no exceptions
                // await durableClient.TerminateAsync(context.InstanceId, "instance threw exception on validation check");
                // var purgeResult = await durableClient.PurgeInstanceHistoryAsync(context.InstanceId);
                this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                {
                    summary = "workflow started with exception",
                    detail = $"{e.Message}",
                    severity = "error",
                    timeStamp = context.CurrentUtcDateTime
                },
                tenantId, oid, entityClient).GetAwaiter().GetResult();

                // var abort = await this.TableRetentionApplianceEngine.GetStateForUpdateWorkflowCheckpoints(entityClient, tenantId, subscriptionId, oid, WorkflowOperation.CancelWorkflow);
                // log.LogWarning("excecution history purged");
                context.SetCustomStatus("exception starting");
                return;
            }

            var storageAccountOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_GetV2StorageAccounts);
            var buildRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_BuildRetentionPolicyTuples);
            var applyRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_ApplyRetentionPolicyTuples);
            var commitRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_CommitRetentionPolicy);
            var cancelWorkflowOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.CANCEL_WORKFLOW);


            Task<ActivityConfig> candidate = null;


            var tasks = new List<Task<ActivityConfig>>()
            {
                storageAccountOperation,cancelWorkflowOperation, buildRetentionPolicyOperation
            };
            var winner1 = await this.TableRetentionApplianceEngine.ActivitiesEngine
                .WhenAnySignalledTask<ActivityConfig>(tasks);
            candidate = winner1;
            var currentActivityConfig = await candidate;
            //var storageActivityConfig = new ActivityConfig();
            //storageActivityConfig.ActivityContext = appcontext;
            if (currentActivityConfig.WorkflowOperation != null && (currentActivityConfig.WorkflowOperation.CommandCode == WorkflowOperation.GetV2StorageAccounts)) // && !context.IsReplaying)
            {
                log.LogInformation("user sent device get v2 storage accounts");
                this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                {
                    summary = "workflow running",
                    detail = "user sent device get v2 storage accounts",
                    severity = "info",
                    timeStamp = context.CurrentUtcDateTime,
                    ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                },
                tenantId, oid, entityClient).GetAwaiter().GetResult();

                try
                {
                    var activityParm = new ActivityConfig()
                    {
                        ActivityContext = currentActivityConfig.ActivityContext,
                        AuthToken = currentActivityConfig.AuthToken,
                        WorkflowOperation = currentActivityConfig.WorkflowOperation
                    };

                    var activityConfigJson = JsonConvert.SerializeObject(activityParm);
                    var storageAccountsResult = await
                        context.CallActivityAsync
                            <ApplianceSessionContextEntity>(ControlChannelConstants
                            // jsonconvert workaround for serialization bug for inputs to this activity
                            .GetV2StorageAccounts, activityConfigJson);

                    // we want to continue as new with the latest
                    appcontext = storageAccountsResult;
                    this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "workflow running",
                        detail = "getting v2 storage accounts",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                    },
                    tenantId, oid, entityClient).GetAwaiter().GetResult();


                    context.SetCustomStatus("GetAllV2StorageAccountsEndpoint");

                }
                catch (Exception e)
                {
                    log.LogError("exception getting v2 storage accounts {0}", e.Message);
                }

                //context.SetCustomStatus((await winner1).WorkflowOperation.DisplayMessage);

            }
            else if (currentActivityConfig.WorkflowOperation != null && (currentActivityConfig.WorkflowOperation.CommandCode == WorkflowOperation.BuildEnvironmentRetentionPolicy)) // && !context.IsReplaying)
            {
                try
                {
                    log.LogInformation("user sent device build retention policy");

                    try
                    {
                        var json = JsonConvert.SerializeObject(currentActivityConfig);
                        int i = 0;
                    }
                    catch (Exception e)
                    {
                        int i = 0;
                    }
                    var policyTuplesResults = await context.CallActivityAsync<ApplianceSessionContextEntity>(ControlChannelConstants.BuildRetentionPolicyTuplesEndpoint, currentActivityConfig);
                    appcontext = policyTuplesResults;

                    this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "workflow running",
                        detail = "building retention policy",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient).GetAwaiter().GetResult();
                    context.SetCustomStatus("BuildEnvironmentRetentionPolicy");
                    log.LogInformation("called building environment activity");
                }
                catch (Exception e)
                {
                    log.LogError("exception building retention policy {0}", e.Message);
                }
            }
            else if (currentActivityConfig.WorkflowOperation != null && (currentActivityConfig.WorkflowOperation.CommandCode == WorkflowOperation.ApplyEnvironmentRetentionPolicy)) //&& !context.IsReplaying)
            {
                try
                {
                    log.LogInformation("user sent device apply retention policy");
                    this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "workflow running",
                        detail = "applying retention policy to calculate retention urface",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient).GetAwaiter().GetResult();
                    var currentCtx = await context.CallActivityAsync<ApplianceSessionContextEntity>(ControlChannelConstants.ApplyTableEntityRetentionPolicyTuplesEndpoint, currentActivityConfig);
                    appcontext = currentCtx;

                    context.SetCustomStatus("ApplyEnvironmentRetentionPolicy");
                }
                catch (Exception e)
                {
                    log.LogError("exception applying retention policy {0}", e.Message);
                }
            }

            // Initializegate(context, out storageAccountOperation, out buildRetentionPolicyOperation, out applyRetentionPolicyOperation, out commitRetentionPolicyOperation, out cancelWorkflowOperation, out tasks);
            // tasks.Add(buildRetentionPolicyOperation);

            candidate = winner1;
            var winner = await candidate;


            if ((winner != null && winner.WorkflowOperation != null)
                && (winner.WorkflowOperation.CommandCode
                        != WorkflowOperation.CancelWorkflow)
                && winner.WorkflowOperation.CommandCode
                        != WorkflowOperation.CommitRetentionPolicyConfiguration)
            {

                this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                {
                    summary = "workflow running",
                    detail = "workflow is waiting for new commands",
                    severity = "info",
                    timeStamp = context.CurrentUtcDateTime,
                    ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                },
                  tenantId, oid, entityClient).GetAwaiter().GetResult();

                context.ContinueAsNew(appcontext);
            }
            else
            {
                this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                {
                    summary = "workflow ended",
                    detail = "workflow has ended",
                    severity = "info",
                    timeStamp = context.CurrentUtcDateTime,
                    ExecutedCommand = currentActivityConfig.WorkflowOperation.CandidateCommand
                },
                  tenantId, oid, entityClient).GetAwaiter().GetResult();
                context.SetCustomStatus("finished");

            }
        }

        [FunctionName(ControlChannelConstants.GetCurrentApplianceContextActivityEndpoint)]
        public async Task<ApplianceSessionContextEntity> GetApplianceSessionContextActivity(
            [ActivityTrigger] IDurableActivityContext activityContext,
            [DurableClient] IDurableEntityClient entityClient, ILogger log)
        {
            ApplianceSessionContextEntity ret = new ApplianceSessionContextEntity();

            var input = new ActivityConfig();
            try
            {
                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "appliance session context",
                        detail = "workflow will retrieve appliance session context.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime
                    },
                tenantId, oid, entityClient);

                log.LogInformation("applying retention policy tuples. getting activity input");
                input = activityContext.GetInput<ActivityConfig>();
                if (input != null && input.ActivityContext != null)
                {
                    var tenantid = input.ActivityContext.TenantId;
                    var userOid = input.ActivityContext.UserOid;
                    var token = input.AuthToken;
                    var entityId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantid, userOid);
                    var entity = await entityClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(entityId);
                    var entityJson = JsonConvert.SerializeObject(entity.EntityState);
                    ret = JsonConvert.DeserializeObject<ApplianceSessionContextEntity>(entityJson);

                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "appliance session context",
                        detail = "workflow has retrieved appliance session context.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime
                    },
                tenantId, oid, entityClient);
                }
            }
            catch (Exception e)
            {
                log.LogError($"exception getting current session context {e.Message}");
                                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "exception: appliance session context",
                        detail = e.Message,
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime
                    },
                tenantId, oid, entityClient);
            }

            return ret;
        }

        private static void Initializegate(IDurableOrchestrationContext context, out Task<ActivityConfig> storageAccountOperation, out Task<ActivityConfig> buildRetentionPolicyOperation, out Task<ActivityConfig> applyRetentionPolicyOperation, out Task<ActivityConfig> commitRetentionPolicyOperation, out Task<ActivityConfig> cancelWorkflowOperation, out List<Task<ActivityConfig>> tasks)
        {
            storageAccountOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_GetV2StorageAccounts);
            buildRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_BuildRetentionPolicyTuples);
            applyRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_ApplyRetentionPolicyTuples);
            commitRetentionPolicyOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.WorkflowEvent_CommitRetentionPolicy);
            cancelWorkflowOperation = context.WaitForExternalEvent<ActivityConfig>(ControlChannelConstants.CANCEL_WORKFLOW);
            tasks = new List<Task<ActivityConfig>>()
            {
                storageAccountOperation,cancelWorkflowOperation, buildRetentionPolicyOperation
            };

            await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "workflow",
                        detail = "workflow initialized menu selection state machine.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
        }

        [Obsolete]
        [FunctionName(ControlChannelConstants.WorkflowEntryPoint)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Argument", "DurableFunctionOrchestrationTriggerAnalyzer:Orchestration Trigger Usage", Justification = "<Pending>")]
        public async Task DefaultEnvronmentDiscoveryWorkflow([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>> output = new List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>();
            List<TableStorageEntityRetentionPolicyEnforcementResult> output = new List<TableStorageEntityRetentionPolicyEnforcementResult>();


            // exceptions in activities are marshalled back here
            // forming a sort of transactional context
            // as per https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-error-handling?tabs=csharp
            try
            {
                // workflow signal gate
                WorkflowOperation storageAccountOperation = await context.WaitForExternalEvent<WorkflowOperation>(ControlChannelConstants.WorkflowEvent_GetV2StorageAccounts);

                var storageAccountsResult = await context.CallActivityAsync<List<StorageAccountModel>>(ControlChannelConstants.GetAllV2StorageAccountsEndpoint, context.InstanceId);

                // workflow signal gate
                WorkflowOperation buildTuplesOperation = await context.WaitForExternalEvent<WorkflowOperation>(ControlChannelConstants.WorkflowEvent_BuildRetentionPolicyTuples);

                var policyTuplesResults = await context.CallActivityAsync<List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>>(ControlChannelConstants.BuildRetentionPolicyTuplesEndpoint, storageAccountsResult);

                foreach (var policyTupleResult in policyTuplesResults)
                {
                    policyTupleResult.Item1.TableStorageEntityRetentionPolicy.NumberOfDays = 10;

                    policyTupleResult.Item1.TableStorageTableRetentionPolicy.DeleteOlderTablesThanCurrentMonthMinusThis = 2;

                }

                var policyTupleStatus = JsonConvert.SerializeObject(policyTuplesResults);
                JObject policyTupleStatuswrapper = TableRetentionApplianceEngine.GetCustomEventForJObject(policyTupleStatus);
                context.SetCustomStatus(policyTupleStatuswrapper);

                // workflow signal gate
                WorkflowOperation applyTuplesOperation = await context.WaitForExternalEvent<WorkflowOperation>(ControlChannelConstants.WorkflowEvent_ApplyRetentionPolicyTuples);

                List<TableStorageEntityRetentionPolicyEnforcementResult> result = new List<TableStorageEntityRetentionPolicyEnforcementResult>();
                result = await context.CallActivityAsync<List<TableStorageEntityRetentionPolicyEnforcementResult>>(ControlChannelConstants.ApplyTableEntityRetentionPolicyTuplesEndpoint, policyTuplesResults);
                JObject policyStatus = new JObject(JsonConvert.SerializeObject(result));
                JObject wrapper = this.TableRetentionApplianceEngine.GetCustomEventForJObject(policyTupleStatus);
                context.SetCustomStatus(wrapper);

                output = result;
            }
            catch (Exception e)
            {
                TelemetryClient.TrackException(e);
            }

            // clients can use the api to observe whether or not this checkpoint
            // has been reached
            // as per https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-custom-orchestration-status?tabs=csharp

            context.SetCustomStatus(ControlChannelConstants.DiscoveryStatus_Finished);
            // return output;
        }

        [FunctionName(ControlChannelConstants.ApplyTableEntityRetentionPolicyTuplesEndpoint)]
        public async Task<ApplianceSessionContextEntity> ApplyRetentionPolicy(
            [ActivityTrigger] IDurableActivityContext context,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            ApplianceSessionContextEntity ret = new ApplianceSessionContextEntity();

            var input = new ActivityConfig();
            List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> policies = new List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>();
            try
            {
                log.LogInformation("applying retention policy tuples. getting activity input");

                
                input = context.GetInput<ActivityConfig>();
                var jobOutput = await input.ActivityContext.GetCurrentJobOutput();

                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow will apply calculated retention policy.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                policies = jobOutput.retentionPolicyJobs.Select(s => s.SourceTuple).ToList();

                foreach (var policyTuple in policies)
                {
                    log.LogInformation(string.Format("applying Retention Policy Tuples"));
                                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = $"applying retention policy to storage account: {policyTuple.Item2.Name}",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                    tenantId, oid, entityClient);

                    TableStorageEntityRetentionPolicyEntity tableStorageEntityRetentionPolicy = policyTuple.Item1.TableStorageEntityRetentionPolicy;
                    TableStorageTableRetentionPolicyEntity tableStorageTableRetentionPolicy = policyTuple.Item1.TableStorageTableRetentionPolicy;

                    StorageAccountEntity storageAccount = policyTuple.Item2;
                    var entityResult = new TableStorageEntityRetentionPolicyEnforcementResultEntity();
                    var tableResult = new TableStorageTableRetentionPolicyEnforcementResultEntity();
                    try
                    {
                        entityResult = await this.TableRetentionApplianceActivities.ApplyTableStorageEntityRetentionPolicy(input.AuthToken, storageAccount, tableStorageEntityRetentionPolicy,
                            () =>
                            {
                                // as per https://stackoverflow.com/questions/62702683/activity-function-needs-current-time-azure-functions
                                return "0" + DateTime.UtcNow.Subtract(new TimeSpan(tableStorageEntityRetentionPolicy.NumberOfDays, 0, 0, 0)).Ticks.ToString();
                            });

                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = $"metrics retention policy applied to {storageAccount.Name}. Policy triggered {entityResult.PolicyTriggerCount} times",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                    }
                    catch (Exception e)
                    {
                    
                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "metrics retention policy exception",
                        detail = $"affected storage account {storageAccount.Name}: message: {e.Message}",
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                        tenantId, oid, entityClient);
                        log.LogError("exception applying policy {0}", e.Message);
                    }

                    try
                    {
                        tableResult = await this.TableRetentionApplianceActivities.ApplyTableStorageTableRetentionPolicy(input.AuthToken, storageAccount, tableStorageTableRetentionPolicy);
                        
                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = $"diagnostics retention policy applied to {storageAccount.Name}. Policy triggered {entityResult.PolicyTriggerCount} times",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                    }
                    catch (Exception e)
                    {
                        log.LogError("exception applying policy {0}", e.Message);
                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "diagnostics retention policy exception",
                        detail = $"affected storage account {storageAccount.Name}: message: {e.Message}",
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                        tenantId, oid, entityClient);

                    }

                    try
                    {
                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow will persist retention policy results.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                        // persist the job output
                        var contextId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<ApplianceSessionContextEntity>(input.ActivityContext.TenantId, input.ActivityContext.UserOid);
                        var currentState = await entityClient.ReadEntityStateAsync<IApplianceSessionContextEntity>(contextId);
                        var currentJobOutput = await currentState.EntityState.GetCurrentJobOutput();
                        var jobHistory = await currentState.EntityState.GetJobOutputHistory();
                        // currentJobOutput.TableEntityRetentionResult = entityResult;
                        currentJobOutput.retentionPolicyJobs.Add(new RetentionPolicyTupleContainerEntity()
                        {
                            Id = Guid.NewGuid(),
                            SourceTuple = policyTuple,
                            StorageAccount = storageAccount,
                            TableStorageEntityPolicyEnforcementResult = entityResult,
                            TableStoragePolicyEnforcementResult = tableResult,
                            TableStorageRetentionPolicy = policyTuple.Item1

                        });

                        jobHistory.Add(currentJobOutput);

                        // await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(contextId, proxy => { proxy.SetCurrentJobOutput(currentJobOutput); });
                        await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(contextId, proxy => { proxy.SetJobOutputHistory(jobHistory); });

                        var currentCtx = await entityClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(contextId);
                        ret = currentCtx.EntityState;
                    await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow HAS persiStED retention policy results.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);

                    }
                    catch (Exception e)
                    {
                        log.LogError("problem persisting result of applied tuples {0}", e.Message);
                        await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "persist retention policy exception",
                        detail = $"affected storage account {storageAccount.Name}: message: {e.Message}",
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                        tenantId, oid, entityClient);
                    }

                    log.LogInformation(string.Format("applied Retention Policy Tuples"));

                }


            }
            catch (Exception e)
            {
                log.LogError("problem with input to apply retention policy activity {0}", e.Message);
            await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "apply retention policy exception",
                        detail = $"affected storage account {storageAccount.Name}: message: {e.Message}",
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = input.WorkflowOperation.CandidateCommand
                    },
                        tenantId, oid, entityClient);
                return ret;
            }


            return ret;

        }



        [FunctionName(ControlChannelConstants.BuildRetentionPolicyTuplesEndpoint)]
        public async Task<ApplianceSessionContextEntity> BuildRetentionPolicyTuples(
            [DurableClient] IDurableEntityClient entityClient,
            [ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var ret = new ApplianceSessionContextEntity();
            try
            {

                var activityConfig = context.GetInput<ActivityConfig>();
                var tenantId = activityConfig.ActivityContext.TenantId;
                var oid = activityConfig.ActivityContext.UserOid;
                var token = activityConfig.AuthToken;
                
                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow is generating retention policy meta data.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                log.LogTrace("build retention policy tuples: using token {0}", token);
                log.LogTrace("build retention policy tuples: using tenantId {0}", tenantId);
                log.LogTrace("build retention policy tuples: using oid {0}", oid);
                log.LogTrace("build retention policy tuples: using token {0}", token);

                var result = await this.TableRetentionApplianceEngine
                                        .ActivitiesEngine
                                        .RenderRetentionPolicyTuples(token,
                                                await activityConfig.ActivityContext.GetSelectedStorageAccounts(),
                                                activityConfig.ActivityContext);

                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow has generated retention policy meta data.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                var entityId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<ApplianceSessionContextEntity>(tenantId, oid);
                var currentCtx = await this.TableRetentionApplianceEngine.ActivitiesEngine.InitializeCurrentJobOutput(entityId,
                    entityClient, result);
                ret = currentCtx;

                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy",
                        detail = "workflow has persisted retention policy meta data.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                log.LogTrace("completed BuildRetentionPolicyTuples activity");

            }
            catch (Exception e)
            {
                 await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "retention policy exception",
                        detail = e.Message,
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                log.LogError("problem getting tuples {0}", e.Message);
            }

            Debug.WriteLine(string.Format("Built Retention Policy Tuples"));

            return ret;

        }



        [FunctionName(ControlChannelConstants.GetAllV2StorageAccountsEndpoint)]
        public async Task<ApplianceSessionContextEntity> GetAllV2StorageAccountsDeprecated(
            [ActivityTrigger] ActivityConfig activityConfig)
        {

            int i = 0;
            return await Task.FromResult<ApplianceSessionContextEntity>(new ApplianceSessionContextEntity());
        }

        [FunctionName(ControlChannelConstants.GetV2StorageAccounts)]
        public async Task<ApplianceSessionContextEntity> GetV2StorageAccountsActivity(
            // [ActivityTrigger] IDurableActivityContext context, 
            [ActivityTrigger] string activityConfigJson,
            [DurableClient] IDurableEntityClient entityClient)
        {
            var ret = new ApplianceSessionContextEntity();
            // var activityConfigJson = context.GetInput<string>();
            var activityConfig = JsonConvert.DeserializeObject<ActivityConfig>(activityConfigJson);
            var tenantId = activityConfig.ActivityContext.TenantId;
            var oid = activityConfig.ActivityContext.UserOid;
            var token = activityConfig.AuthToken;
            await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "storage accounts",
                        detail = "workflow is validating meta data for selected storage accounts.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);

            // TODO - make a RESET current job output -
            // clear the previous workflow results for this step
            //await this.TableRetentionApplianceEngine.ActivitiesEngine.UpdateApplianceJobOutput(entityId,
            //    entityClient, new List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>());

            try
            {
                // todo make this robust
                var accounts = await this.TableRetentionApplianceEngine.ActivitiesEngine
                            .GetStorageAccountsForUser(tenantId, oid, token, activityConfig.ActivityContext);

                var countMatches = accounts.Count == activityConfig.ActivityContext.SelectedStorageAccounts.Count;
                if (!countMatches)
                {
                    // big TODO react  to this
                    log.LogWarning("did not get all hte accounts the user selected");
                }

                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "storage accounts",
                        detail = "workflow has validated storage account meta data.",
                        severity = "info",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
            }
            catch (Exception e)
            {
                await this.TableRetentionApplianceEngine.Log(new JobOutputLogEntry()
                    {
                        summary = "storage account exception",
                        detail = e.Message,
                        severity = "error",
                        timeStamp = context.CurrentUtcDateTime,
                        ExecutedCommand = activityConfig.WorkflowOperation.CandidateCommand
                    },
                tenantId, oid, entityClient);
                log.LogError($"exception getting storage accounts {e.Message}");
            }

            var ctxId = CrucialExtensions.HashToGuid(tenantId, oid);
            var ctxEntitId = new EntityId(nameof(IApplianceSessionContextEntity), ctxId.ToString());

            var ctx = await entityClient.ReadEntityStateAsync<ApplianceSessionContextEntity>(ctxEntitId);
            ret = ctx.EntityState;
            return ret;
        }



        public async Task<ApplianceJobOutputEntity> ResetCurrentJobOutput(IDurableEntityClient entityClient, ApplianceSessionContextEntity context)
        {
            var ret = new ApplianceJobOutputEntity();
            try
            {
                var entityId = await this.TableRetentionApplianceEngine.GetEntityIdForUser<ApplianceSessionContextEntity>(context.TenantId, context.UserOid);
                await entityClient.SignalEntityAsync<IApplianceSessionContextEntity>(entityId, proxy =>
                {
                    proxy.SetCurrentJobOutput(new ApplianceJobOutputEntity());
                });
            }
            catch (Exception e)
            {
                log.LogError("problem resetting current job output {0}", e.Message);
            }

            return ret;
        }


        [Obsolete]
        public async Task<List<StorageAccountModel>> GetAllV2StorageAccounts([ActivityTrigger] IDurableActivityContext context)
        {
            Debug.WriteLine(string.Format("Getting All Storage Accounts "));
            List<StorageAccountModel> ret = new List<StorageAccountModel>();
            var activityConfig = context.GetInput<ActivityConfig>();

            try
            {
                // get them all


                ret = await TableEntityRetentionClient.GetAllV2StorageAccounts();
                // TableEntityRetentionClient.CachedV2StorageAccounts = result;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         

            }
            catch (Exception e)
            {
                TelemetryClient.TrackException(e);
            }

            Debug.WriteLine(string.Format("Got All V2 Storage Accounts"));

            return ret;
        }

        /// <summary>
        /// as per https://www.koskila.net/how-to-access-azure-function-apps-settings-from-c/
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            // as per 
            var config = new ConfigurationBuilder()
                  .SetBasePath(context.FunctionAppDirectory)
                  // This gives you access to your application settings in your local development environment
                  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  // This is what actually gets you the application settings in Azure
                  .AddEnvironmentVariables()
                  .Build();
            return config;
        }

    }
}