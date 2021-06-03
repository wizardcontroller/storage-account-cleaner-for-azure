using com.ataxlab.azure.table.retention.models.models.auth;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.models
{


    public class DurableOrchestrationStateDTO
    {
        public string name { get; set; }
        [Key]
        public string instanceId { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime lastUpdatedTime { get; set; }
        public string input { get; set; }
        public string output { get; set; }
        public ApplianceSessionContext InputApplianceContext 
        {
            get
            {
                ApplianceSessionContext ret = new ApplianceSessionContext();
                if(!String.IsNullOrEmpty(input))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<ApplianceSessionContext>(input);
                    }
                    catch(Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine("workflow input is not as expected " + e.Message);
                    }
                }

                return ret;
            }
        }

        public OrchestrationRuntimeStatus runtimeStatus { get; set; }
        public string customStatus { get; set; }
        public string history { get; set; }
    }

    public class DurableOrchestrationStatusDTO
    {
        //[Key]
        public string Id { get; set; }
        [JsonProperty("durableOrchestrationState")]
        public List<DurableOrchestrationStateDTO> durableOrchestrationState { get; set; }

    }


}
