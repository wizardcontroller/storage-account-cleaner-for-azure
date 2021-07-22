using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace com.ataxlab.functions.table.retention.entities
{
    public interface IJobOutputLogEntity
    {
        void appendLog(JobOutputLogEntry logEntry);
        Task<List<JobOutputLogEntry>> getLogEntries(int startoffset, int pageCount, int pageSize);
        Task<int> getLogEntryCount();
    }

    [JsonObject(MemberSerialization.OptOut, NamingStrategyType = typeof(CamelCaseNamingStrategy))]

    public class JobOutputLogEntity : JobOutputLogEntityBase, IJobOutputLogEntity
    {

        public JobOutputLogEntity() : base()
        {
            logEntries = new List<JobOutputLogEntry>();
        }

        public void appendLog(JobOutputLogEntry logEntry)
        {
            logEntries.Add(logEntry);
        }

        public async Task<List<JobOutputLogEntry>> getLogEntries(int startoffset, int pageCount, int pageSize)
        {
            var ret = new List<JobOutputLogEntry>();
            var outputList = this.logEntries.Skip(startoffset).Take(pageCount * pageSize).ToList< JobOutputLogEntry>();
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
}
