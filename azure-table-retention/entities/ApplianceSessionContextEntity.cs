using com.ataxlab.azure.table.retention.models.models.auth;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using Newtonsoft.Json;
using com.ataxlab.functions.table.retention.entities;
using com.ataxlab.functions.table.retention.services;
using com.ataxlab.azure.table.retention.models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace com.ataxlab.azure.table.retention.state.entities
{


    public interface IApplianceSessionContextEntity
    {
        void Delete();
        Task<ApplianceJobOutputEntity> GetCurrentJobOutput();
        Task<string> GetId();
        Task<List<ApplianceJobOutputEntity>> GetJobOutputHistory();
        Task<List<StorageAccountEntity>> GetSelectedStorageAccounts();
        Task<string> GetSelectedSubscriptionId();
        Task<string> GetTenantId();
        Task<string> GetUserOid();
        void SetCurrentJobOutput(ApplianceJobOutputEntity output);

        void InitializeCurrentJobOutput(List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> tuples);

        Task<TableStorageTableRetentionPolicyEnforcementResultEntity> GetCurrentTableStoragTableRetentionPolicyEnforcementResult(StorageAccountEntity storageAccount);

        Task<TableStorageEntityRetentionPolicyEnforcementResultEntity> GetCurrentTableStorageEntityRetentionPolicyEnforcementResult(StorageAccountEntity storageAccount);


        void SetId(string id);
        void SetJobOutputHistory(List<ApplianceJobOutputEntity> jobOutput);
        void SetSelectedStorageAccounts(List<StorageAccountEntity> accounts);
        void SetSelectedSubscriptionId(string id);
        void SetTenantId(string tenantId);
        void SetUserOid(string oid);
        Task<List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>> GetCurrentPolicyTuples();
        Task<TableStorageEntityRetentionPolicyEntity> GetCurrentDiagnosticsRetentionPolicy(StorageAccountEntity storageAccount);
        Task<TableStorageTableRetentionPolicyEntity> GetCurrentMetricsRetentionPolicy(StorageAccountEntity storageAccount);
        void SetTimeStamp(DateTime utcNow);
    }

    /// <summary>
    /// represents the durable entity associated with 
    /// the appliance session context dto
    /// </summary>
    // public class ApplianceSessionContextEntity : ApplianceSessionContext, IApplianceSessionContextEntity
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplianceSessionContextEntity : ApplianceSessionContextEntityBase, IApplianceSessionContextEntity
    {

        public ApplianceSessionContextEntity() : base()
        {

        }

        public void SetTimeStamp(DateTime utcNow)
        {
            TimeStamp = utcNow;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(ApplianceSessionContextEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {

            return ctx.DispatchAsync<ApplianceSessionContextEntity>();
        }

        public Task<List<StorageAccountEntity>> GetSelectedStorageAccounts()
        {
            return Task.FromResult(SelectedStorageAccounts);
        }


        /// <summary>
        /// useful for situations when you need to know storage accounts
        /// without other context information, like storage policy tuples
        /// </summary>
        /// <param name="accounts"></param>
        public void SetSelectedStorageAccounts(List<StorageAccountEntity> accounts)
        {
            SelectedStorageAccounts = accounts;
        }

        public async Task<string> GetUserOid()
        {
            return await Task.FromResult(UserOid);
        }

        public void SetUserOid(string oid)
        {
            UserOid = oid;
        }

        public async Task<string> GetSelectedSubscriptionId()
        {
            return await Task.FromResult(SelectedSubscriptionId);
        }

        public void SetSelectedSubscriptionId(string id)
        {
            SelectedSubscriptionId = id;
        }

        public async Task<string> GetTenantId()
        {
            return await Task.FromResult(TenantId);
        }

        public void SetTenantId(string tenantId)
        {
            TenantId = tenantId;
        }
        public async Task<string> GetId()
        {
            return await Task.FromResult(Id);
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void SetJobOutputHistory(List<ApplianceJobOutputEntity> jobOutput)
        {
            JobOutput = jobOutput;
        }

        public async Task<List<ApplianceJobOutputEntity>> GetJobOutputHistory()
        {
            return await Task.FromResult(JobOutput);
        }

        public async Task<ApplianceJobOutputEntity> GetCurrentJobOutput()
        {
            return await Task.FromResult<ApplianceJobOutputEntity>(CurrentJobOutput);
        }

        public void SetCurrentJobOutput(ApplianceJobOutputEntity output)
        {
            CurrentJobOutput = output;
        }

        /// <summary>
        /// initialize retention policy job containers
        /// one per storage account (tuple)
        /// </summary>
        /// <param name="tuples"></param>
        public void InitializeCurrentJobOutput(List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>> tuples)
        {
            CurrentJobOutput.RetentionPolicyJobs.Clear();

            if (tuples == null) return;

            try
            {
                foreach (var tuple in tuples)
                {
                    try
                    {
                        CurrentJobOutput.RetentionPolicyJobs.Add(new RetentionPolicyTupleContainerEntity()
                        {
                            Id = Guid.NewGuid(),
                            SourceTuple = tuple,
                            StorageAccount = tuple.Item2,
                            TableStorageRetentionPolicy = new TableStorageRetentionPolicyEntity()
                            {
                                Id = Guid.NewGuid(),
                                TableStorageEntityRetentionPolicy = tuple.Item1.TableStorageEntityRetentionPolicy,
                                TableStorageTableRetentionPolicy = tuple.Item1.TableStorageTableRetentionPolicy
                            }

                        });
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine("problem setting policy tuples " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("problem setting policy tuples " + e.Message);
            }
        }

        public async Task<TableStorageTableRetentionPolicyEntity> GetCurrentMetricsRetentionPolicy(StorageAccountEntity storageAccount)
        {
            var result = new TableStorageTableRetentionPolicyEntity();
            if(storageAccount == null)
            {
                return result;
            }

            try
            {
                var poisonedOutput = CurrentJobOutput.RetentionPolicyJobs.FindAll(f => f.StorageAccount.Id == null);
                foreach (var item in poisonedOutput)
                {
                    System.Diagnostics.Trace.TraceError("warning: poisoned properties found");
                    CurrentJobOutput.RetentionPolicyJobs.Remove(item);
                }
            }
            catch (Exception e)
            {

            }


            try
            {
                // expect only 1
                var matchingAccount = CurrentJobOutput.RetentionPolicyJobs.Find(f => f.StorageAccount.Id.Equals(storageAccount.Id));
                if (matchingAccount != null && matchingAccount.Id.Equals(storageAccount.Id))
                {
                    result = matchingAccount.TableStorageRetentionPolicy.TableStorageTableRetentionPolicy;
                }
                else
                {
                    System.Diagnostics.Trace.TraceError("warning: found uninitialized retention policy jobs");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("problem getting policy " + e.Message);
            }

            return await Task.FromResult<TableStorageTableRetentionPolicyEntity>(result);
        }


        public async Task<TableStorageEntityRetentionPolicyEntity> GetCurrentDiagnosticsRetentionPolicy(StorageAccountEntity storageAccount)
        {
            var result = new TableStorageEntityRetentionPolicyEntity();
            if (storageAccount == null)
            {
                return result;
            }

            // side effect - if nulls in collection, clear them
            try
            {
                var poisonedOutput = CurrentJobOutput.RetentionPolicyJobs.FindAll(f => f.StorageAccount.Id == null);
                foreach (var item in poisonedOutput)
                {
                    System.Diagnostics.Trace.TraceError("warning: poisoned properties found");
                    CurrentJobOutput.RetentionPolicyJobs.Remove(item);
                }
            }
            catch(Exception e)
            {

            }

            try
            {
                // expect only 1
                var matchingJob = CurrentJobOutput.RetentionPolicyJobs.Find(f => f.StorageAccount.Id.Equals(storageAccount.Id));
                if (matchingJob != null && matchingJob.StorageAccount.Id.Equals(storageAccount.Id))
                {
                    result = matchingJob.TableStorageRetentionPolicy.TableStorageEntityRetentionPolicy;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("problem getting policy " + e.Message);
            }

            return await Task.FromResult<TableStorageEntityRetentionPolicyEntity>(result);
        }

        public async Task<TableStorageTableRetentionPolicyEnforcementResultEntity> GetCurrentTableStoragTableRetentionPolicyEnforcementResult(StorageAccountEntity storageAccount)
        {
            var result = new TableStorageTableRetentionPolicyEnforcementResultEntity();
            try
            {
                var poisonedOutput = CurrentJobOutput.RetentionPolicyJobs.FindAll(f => f.StorageAccount.Id == null);
                foreach (var item in poisonedOutput)
                {
                    System.Diagnostics.Trace.TraceError("warning: poisoned properties found");
                    CurrentJobOutput.RetentionPolicyJobs.Remove(item);
                }
            }
            catch (Exception e)
            {

            }

            try
            {
                // expect only 1
                var matchingJob = CurrentJobOutput.RetentionPolicyJobs.Find(f => f.StorageAccount.Id.Equals(storageAccount.Id));
                if (matchingJob != null && matchingJob.StorageAccount.Id.Equals(storageAccount.Id))
                {
                    result = matchingJob.TableStoragePolicyEnforcementResult;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("problem getting policy enforcement result " + e.Message);
            }

            return await Task.FromResult<TableStorageTableRetentionPolicyEnforcementResultEntity>(result);
        }

        public async Task<TableStorageEntityRetentionPolicyEnforcementResultEntity> GetCurrentTableStorageEntityRetentionPolicyEnforcementResult(StorageAccountEntity storageAccount)
        {
            var result = new TableStorageEntityRetentionPolicyEnforcementResultEntity();

            if (storageAccount == null)
            {
                return result;
            }
            try
            {
                var poisonedOutput = CurrentJobOutput.RetentionPolicyJobs.FindAll(f => f.StorageAccount.Id == null);
                foreach (var item in poisonedOutput)
                {
                    System.Diagnostics.Trace.TraceError("warning: poisoned properties found");
                    CurrentJobOutput.RetentionPolicyJobs.Remove(item);
                }
            }
            catch (Exception e)
            {

            }


            try
            {
                // expect only 1
                var matchingJob = CurrentJobOutput.RetentionPolicyJobs.Find(f => f.StorageAccount.Id.Equals(storageAccount.Id));
                if (matchingJob != null && matchingJob.StorageAccount.Id.Equals(storageAccount.Id))
                {
                    result = matchingJob.TableStorageEntityPolicyEnforcementResult;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("problem getting policy enforcement result " + e.Message);
            }

            return await Task.FromResult<TableStorageEntityRetentionPolicyEnforcementResultEntity>(result);
        }



        public async Task<List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>> GetCurrentPolicyTuples()
        {
            var ret = new List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>();
            try
            {
                var poisonedOutput = CurrentJobOutput.RetentionPolicyJobs.FindAll(f => f.StorageAccount.Id == null);
                foreach (var item in poisonedOutput)
                {
                    System.Diagnostics.Trace.TraceError("warning: poisoned properties found");
                    CurrentJobOutput.RetentionPolicyJobs.Remove(item);
                }
            }
            catch (Exception e)
            {

            }


            try
            {
                foreach (var job in CurrentJobOutput.RetentionPolicyJobs)
                {
                    ret.Add(job.SourceTuple);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.Write("problem getting policy tuples " + e.Message);
            }

            return await Task.FromResult<List<Tuple<TableStorageRetentionPolicyEntity, StorageAccountEntity>>>(ret);
        }

    }

    public enum AppliancePersistResultType { SUCEEDED, FAILED }

    [JsonObject(MemberSerialization.OptOut)]
    public class ApplianceContextPersistResult
    {
        public AppliancePersistResultType PersistResult { get; set; }
        public string ErrorMessage { get; set; }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class ApplianceSessionContextEntityBase
    {
        public ApplianceSessionContextEntityBase()
        {

            SelectedStorageAccounts = new List<StorageAccountEntity>();
            JobOutput = new List<ApplianceJobOutputEntity>();
            CurrentJobOutput = new ApplianceJobOutputEntity();

        }


        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// represents a hash of all the other properties
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("SelectedSubscriptionId")]
        public string SelectedSubscriptionId { get; set; }


        [JsonProperty("SelectedStorageAccounts")]
        public List<StorageAccountEntity> SelectedStorageAccounts { get; set; }

        [JsonProperty("UserOid")]
        public string UserOid { get; set; }

        [JsonProperty("TenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// what happened last time the appliance tried
        /// to crud the entity
        /// </summary>
        [JsonProperty("OperationResult")]
        public ApplianceContextPersistResult OperationResult { get; set; }

        [JsonProperty("JobOutput")]
        public List<ApplianceJobOutputEntity> JobOutput { get; set; }

        [JsonProperty("CurrentJobOutput")]
        public ApplianceJobOutputEntity CurrentJobOutput { get; set; }

    }
}
