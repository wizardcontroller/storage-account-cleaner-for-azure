using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.functions.table.retention.entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.state.entities
{
    public interface IWorkflowCheckpointProperties
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }

        public string SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public List<AvailableCommandEntity> AvailableCommands { get; set; }

        void Delete();
    }

    /// <summary>
    /// workflows need checkpoints apparently
    /// especially those running headless
    /// </summary>
    public interface IWorkflowCheckpoint
    {
        /// <summary>
        /// state machine management
        /// at each checkpoint (state) a set of commands (transitions) is available
        /// with an acocompanying message to show the user
        /// </summary>
        // public List<AvailableCommand> AvailableCommands { get; set; }
        // public DateTime TimeStamp { get; set; }
        // public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }
        public void SetAvailableCommands(List<AvailableCommandEntity> newCommands);
        public Task<List<AvailableCommandEntity>> GetAvailableCommands();
        public void SetTimeStamp(DateTime now);
        public Task<DateTime> GetTimeStamp();
        public void SetMessage(string message);
        public Task<string> GetMessage();
        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint); // => CurrentCheckpoint = checkpoint;
        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync(); // => Task.FromResult(CurrentCheckpoint);

        
        public void SetubscriptionId(string subscriptionId);
        public Task<string> GetSubscriptionid();

        public void SetSubscriptionNasme(string subscriptionName);
        public Task<string> GetSubscriptionName();
        void Delete();
    }

    /// <summary>
    /// the live configuration
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class WorkflowCheckpoint : IWorkflowCheckpoint, IWorkflowCheckpointProperties
    {
        [JsonProperty("TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("CurrentCheckpoint")]
        public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }


        [JsonProperty("Messsage")]
        public string Message { get;  set; }

        [JsonProperty("AvailableCommands")]
        public List<AvailableCommandEntity> AvailableCommands { get; set; }

        [JsonProperty("SubscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("SubscriptionName")]
        public string SubscriptionName { get; set; }

        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint) => CurrentCheckpoint = checkpoint;

        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync() => Task.FromResult(CurrentCheckpoint);

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(WorkflowCheckpoint))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
             => ctx.DispatchAsync<WorkflowCheckpoint>();

        public void SetAvailableCommands(List<AvailableCommandEntity> newCommands)
        {
            AvailableCommands = newCommands;
        }

        public void SetTimeStamp(DateTime now)
        {
            TimeStamp = now;
        }

        public void SetMessage(string message)
        {
            Message = message;
        }

        public Task<List<AvailableCommandEntity>> GetAvailableCommands()
        {
            return Task.FromResult(AvailableCommands);
        }

        public Task<DateTime> GetTimeStamp()
        {
            return Task.FromResult(TimeStamp);
        }

        public Task<string> GetMessage()
        {
            return Task.FromResult(Message);
        }

        public void SetubscriptionId(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Task<string> GetSubscriptionid()
        {
            return Task.FromResult(SubscriptionId);
        }

        public void SetSubscriptionNasme(string subscriptionName)
        {
            SubscriptionName = subscriptionName;
        }

        public Task<string> GetSubscriptionName()
        {
            return Task.FromResult(SubscriptionName);
        }
    }

    /// <summary>
    /// the staging configuration
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class WorkflowCheckpointEditMode : IWorkflowCheckpoint, IWorkflowCheckpointProperties
    {
        [JsonProperty("TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("CurrentCheckpoint")]
        public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }


        [JsonProperty("Messsage")]
        public string Message { get; set; }

        [JsonProperty("AvailableCommands")]
        public List<AvailableCommandEntity> AvailableCommands { get; set; }


        [JsonProperty("SubscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("SubscriptionName")]
        public string SubscriptionName { get; set; }


        public void SetAvailableCommands(List<AvailableCommandEntity> newCommands)
        {
            AvailableCommands = newCommands;
        }

        public void SetTimeStamp(DateTime now)
        {
            TimeStamp = now;
        }

        public void SetMessage(string message)
        {
            Message = message;
        }

        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint) => CurrentCheckpoint = checkpoint;

        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync() => Task.FromResult(CurrentCheckpoint);

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(WorkflowCheckpointEditMode))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
             => ctx.DispatchAsync<WorkflowCheckpointEditMode>();


        public Task<List<AvailableCommandEntity>> GetAvailableCommands()
        {
            return Task.FromResult(AvailableCommands);
        }

        public Task<DateTime> GetTimeStamp()
        {
            return Task.FromResult(TimeStamp);
        }

        public Task<string> GetMessage()
        {
            return Task.FromResult(Message);
        }

        public void SetubscriptionId(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Task<string> GetSubscriptionid()
        {
            return Task.FromResult(SubscriptionId);
        }

        public void SetSubscriptionNasme(string subscriptionName)
        {
            SubscriptionName = subscriptionName;
        }

        public Task<string> GetSubscriptionName()
        {
            return Task.FromResult(SubscriptionName);
        }
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class WorkflowCheckpointStatus
    {
        public WorkflowCheckpoint Checkpoint { get; set; }
    }
}
