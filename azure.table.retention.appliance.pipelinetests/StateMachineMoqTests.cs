using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.functions.table.retention.services;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using com.ataxlab.azure.table.retention.models.extensions;
using com.ataxlab.functions.table.retention.entities;
using WorkflowOperation = com.ataxlab.functions.table.retention.entities.WorkflowOperation;

namespace com.ataxlab.functions.table.retention.smoketests
{
    public class StateMachineMoqTests
    {
        ITableRetentionApplianceEngine engine = TableRetentionApplianceEngine.GetUnitTestableEngine();

        Mock<IDurableClient> iDurableClient = new Mock<IDurableClient>();
        public StateMachineMoqTests()
        {
        }

        [Test]
        public async Task MenuGenerationSmokeTest()
        {
            // get a list of the lists of results
            var ResultMenuSet = new List<AvailableCommandEntity>();

            #region smoketest command generation endpoint
            WorkflowOperation ProvisionAppliance = WorkflowOperation.ProvisionAppliance;
            var ProvisionApplianceresult = await engine.GetAvailableCommandsForTransition(ProvisionAppliance);
            Assert.IsNotNull(ProvisionApplianceresult, "failed to get available commands for transition");
            Assert.IsTrue(ProvisionApplianceresult.FindAll(f => f.WorkflowOperation
                                                           == WorkflowOperation.ProvisionAppliance).Count == 1,
                                                           "incorrect command code {0}", await ProvisionApplianceresult.ToJSONStringAsync());
            ResultMenuSet.AddRange(ProvisionApplianceresult);

            var BeginEnvironmentDiscovery =  WorkflowOperation.BeginWorkflow;
            var BeginEnvironmentDiscoveryresult = await engine.GetAvailableCommandsForTransition(BeginEnvironmentDiscovery);
            Assert.IsNotNull(BeginEnvironmentDiscoveryresult, "failed to get available commands for transition");
            Assert.IsTrue(BeginEnvironmentDiscoveryresult.FindAll(f => f.WorkflowOperation
                                                            == WorkflowOperation.GetV2StorageAccounts).Count == 1,
                                                            "incorrect command code {0}", await BeginEnvironmentDiscoveryresult.ToJSONStringAsync());
            ResultMenuSet.AddRange(BeginEnvironmentDiscoveryresult );

            var GetV2StorageAccounts = WorkflowOperation.GetV2StorageAccounts;
            var GetV2StorageAccountsresult = await engine.GetAvailableCommandsForTransition(GetV2StorageAccounts);
            Assert.IsNotNull(GetV2StorageAccountsresult, "failed to get available commands for transition");
            Assert.IsTrue(GetV2StorageAccountsresult.FindAll(f => f.WorkflowOperation 
                                                            == WorkflowOperation.BuildEnvironmentRetentionPolicy).Count == 1,
                                                            "incorrect command code {0}", await GetV2StorageAccountsresult.ToJSONStringAsync());
            
            ResultMenuSet.AddRange(GetV2StorageAccountsresult);

            var BuildEnvironmentRetentionPolicy = WorkflowOperation.BuildEnvironmentRetentionPolicy;
            var BuildEnvironmentRetentionPolicyresult = await engine.GetAvailableCommandsForTransition(BuildEnvironmentRetentionPolicy);
            Assert.IsNotNull(BuildEnvironmentRetentionPolicyresult, "failed to get available commands for transition");
            Assert.IsTrue(BuildEnvironmentRetentionPolicyresult.FindAll(f => f.WorkflowOperation
                                                            == WorkflowOperation.ApplyEnvironmentRetentionPolicy).Count == 1,
                                                            "incorrect command code {0}", await BuildEnvironmentRetentionPolicyresult.ToJSONStringAsync());
            ResultMenuSet.AddRange(BuildEnvironmentRetentionPolicyresult);

            var ApplyEnvironmentRetentionPolicy = WorkflowOperation.ApplyEnvironmentRetentionPolicy;
            var ApplyEnvironmentRetentionPolicyresult = await engine.GetAvailableCommandsForTransition(ApplyEnvironmentRetentionPolicy);
            Assert.IsNotNull(ApplyEnvironmentRetentionPolicyresult, "failed to get available commands for transition");
            Assert.IsTrue(ApplyEnvironmentRetentionPolicyresult.FindAll(f => f.WorkflowOperation
                                                           == WorkflowOperation.CommitRetentionPolicyConfiguration).Count == 1,
                                                           "incorrect command code {0}", await ApplyEnvironmentRetentionPolicyresult.ToJSONStringAsync());
            ResultMenuSet.AddRange(ApplyEnvironmentRetentionPolicyresult);

            var CommitRetentionPolicyConfiguration = WorkflowOperation.CommitRetentionPolicyConfiguration;
            var CommitRetentionPolicyConfigurationresult = await engine.GetAvailableCommandsForTransition(CommitRetentionPolicyConfiguration);
            Assert.IsNotNull(CommitRetentionPolicyConfigurationresult, "failed to get available commands for transition");

            var testResult = CommitRetentionPolicyConfigurationresult.FindAll(f => f.WorkflowOperation == WorkflowOperation.BeginWorkflow);
            Assert.IsTrue(CommitRetentionPolicyConfigurationresult.FindAll(f => f.WorkflowOperation
                                                          == WorkflowOperation.BeginWorkflow).Count == 1,
                                                          "incorrect command code {0}", await CommitRetentionPolicyConfigurationresult.ToJSONStringAsync());
            ResultMenuSet.AddRange(CommitRetentionPolicyConfigurationresult);
            #endregion smoketest command generation endpoint

            #region string validation tests
            var stringtest = ResultMenuSet.FindAll(f => f.MenuLabel == null || f.MenuLabel.Equals(String.Empty) || f.MenuLabel.Length < 2);
            Assert.IsTrue(stringtest.Count == 0, "test failed due to missing menu labels {0}", await stringtest.ToJSONStringAsync());

            stringtest = ResultMenuSet.FindAll(f => f.WorklowOperationDisplayMessage == null || 
                                                f.WorklowOperationDisplayMessage.Equals(String.Empty) || 
                                                f.WorklowOperationDisplayMessage.Length < 2);
            Assert.IsTrue(stringtest.Count == 0, "test failed due to missing WorklowOperationDisplayMessage {0}", await stringtest.ToJSONStringAsync());

            stringtest = ResultMenuSet.FindAll(f => f.AvailableCommandId == null ||
                                                f.AvailableCommandId.Equals(String.Empty) ||
                                                f.AvailableCommandId.Length < 2);
            Assert.IsTrue(stringtest.Count == 0, "test failed due to missing AvailableCommandId {0}", await stringtest.ToJSONStringAsync());
            #endregion string validation tests

        }
    }
}
