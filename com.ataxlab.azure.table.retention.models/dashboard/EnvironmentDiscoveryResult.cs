using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.dashboard
{
    public class EnvironmentDiscoveryResult
    {
        public EnvironmentDiscoveryResult()
        {
            this.Value = new List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>();
            this.TimeStamp = DateTime.UtcNow.ToLongTimeString();
        }

        public string TimeStamp { get; set; }

        public string OrchestrationInstanceId { get; set; }

        public List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>> Value { get; set; }
    }
}
