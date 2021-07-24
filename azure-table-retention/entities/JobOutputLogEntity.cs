using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using com.ataxlab.azure.table.retention.state.entities;
using Microsoft.Azure.WebJobs;

namespace com.ataxlab.functions.table.retention.entities
{
    public interface IJobOutputLogEntity
    {
        void appendLog(JobOutputLogEntry logEntry);
        public Task<List<JobOutputLogEntry>> getLogEntries(LogEntryQuery query);
        Task<int> getLogEntryCount();
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class JobOutputLogEntity : JobOutputLogEntityBase, IJobOutputLogEntity
    {

        [FunctionName(nameof(JobOutputLogEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<JobOutputLogEntity>();
        }

        public JobOutputLogEntity() : base()
        {
            logEntries = new List<JobOutputLogEntry>();
        }

        public void appendLog(JobOutputLogEntry logEntry)
        {
            logEntries.Add(logEntry);
        }

        public async Task<List<JobOutputLogEntry>> getLogEntries(LogEntryQuery query)
        {
            int startoffset = query.startoffset;
            int pageCount = query.pageCount;
            int pageSize = query.pageSize;

            var ret = new List<JobOutputLogEntry>();
            var outputList = this.logEntries.OrderByDescending(o => o.timeStamp).Skip(startoffset).Take(pageCount * pageSize).ToList< JobOutputLogEntry>();
            ret.AddRange(outputList);
            return await Task.FromResult(ret);
        }

        public async Task<int> getLogEntryCount()
        {
            return await Task.FromResult(this.logEntries.Count());
        }
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class JobOutputLogEntityBase
    {
        public string userOid { get; set; }
        public string userTenantId { get; set; }

        public int rowCount { get; set; }

        public List<JobOutputLogEntry> logEntries { get; set; }

    }

    public class LogEntryQuery
    {
        public int startoffset { get; set; } 
        public int pageCount { get; set; } 
        
        public int pageSize { get; set; }
    }
}
