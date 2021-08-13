using com.ataxlab.azure.table.retention.models.control;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.models.models
{


    /// <summary>
    /// workflows need checkpoints apparently
    /// especially those running headless
    /// </summary>
    public interface IWorkflowCheckpointDTO
    {
        /// <summary>
        /// state machine management
        /// at each checkpoint (state) a set of commands (transitions) is available
        /// with an acocompanying message to show the user
        /// </summary>
        // public List<AvailableCommand> AvailableCommands { get; set; }
        // public DateTime TimeStamp { get; set; }
        // public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }
        public void SetAvailableCommands(List<AvailableCommand> newCommands);
        public Task<List<AvailableCommand>> GetAvailableCommands();
        public void SetTimeStamp(DateTime now);
        public Task<DateTime> GetTimeStamp();
        public void SetMessage(string message);
        public Task<string> GetMessage();
        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint); // => CurrentCheckpoint = checkpoint;
        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync(); // => Task.FromResult(CurrentCheckpoint);

    }

    /// <summary>
    /// the live configuration
    /// </summary>
    public class WorkflowCheckpointDTO
    {
        public int WorkflowCheckpointDTOId { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("currentCheckpoint")]
        public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }


        [JsonProperty("messsage")]
        public string Message { get; internal set; }

        [JsonProperty("availableCommands")]
        public List<AvailableCommand> AvailableCommands { get; set; }

        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint) => this.CurrentCheckpoint = checkpoint;

        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync() => Task.FromResult(this.CurrentCheckpoint);

        public void SetAvailableCommands(List<AvailableCommand> newCommands)
        {
            this.AvailableCommands = newCommands;
        }

        public void SetTimeStamp(DateTime now)
        {
            this.TimeStamp = now;
        }

        public void SetMessage(string message)
        {
            this.Message = message;
        }

        public Task<List<AvailableCommand>> GetAvailableCommands()
        {
            return Task.FromResult(this.AvailableCommands);
        }

        public Task<DateTime> GetTimeStamp()
        {
            return Task.FromResult(this.TimeStamp);
        }

        public Task<string> GetMessage()
        {
            return Task.FromResult(this.Message);
        }
    }

    /// <summary>
    /// the staging configuration
    /// </summary>
    public class WorkflowCheckpointEditModeDTO 
    {
        [JsonProperty("TimeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("CurrentCheckpoint")]
        public WorkflowCheckpointIdentifier CurrentCheckpoint { get; set; }


        [JsonProperty("Messsage")]
        public string Message { get; internal set; }

        [JsonProperty("AvailableCommands")]
        public List<AvailableCommand> AvailableCommands { get; set; }

        public void SetAvailableCommands(List<AvailableCommand> newCommands)
        {
            this.AvailableCommands = newCommands;
        }

        public void SetTimeStamp(DateTime now)
        {
            this.TimeStamp = now;
        }

        public void SetMessage(string message)
        {
            this.Message = message;
        }

        public void SetCurrentCheckpoint(WorkflowCheckpointIdentifier checkpoint) => this.CurrentCheckpoint = checkpoint;

        public Task<WorkflowCheckpointIdentifier> GetCurrentCheckpointAsync() => Task.FromResult(this.CurrentCheckpoint);


        public Task<List<AvailableCommand>> GetAvailableCommands()
        {
            return Task.FromResult(this.AvailableCommands);
        }

        public Task<DateTime> GetTimeStamp()
        {
            return Task.FromResult(this.TimeStamp);
        }

        public Task<string> GetMessage()
        {
            return Task.FromResult(this.Message);
        }
    }
    public class WorkflowCheckpointStatus
    {

        public int WorkflowCheckpointStatusId { get; set; }
        public WorkflowCheckpointDTO Checkpoint { get; set; }
    }
}
