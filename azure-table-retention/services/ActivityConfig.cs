using com.ataxlab.azure.table.retention.state.entities;
using com.ataxlab.functions.table.retention.entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.services
{
    [JsonObject]
    public class ActivityConfig
    {
        public WorkflowOperationCommandEntity WorkflowOperation { get; set; }
        public ApplianceSessionContextEntity ActivityContext { get; set; }

        public string AuthToken { get; set; }
    }
}
