using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.azure.table.retention.models.models;
using com.ataxlab.azure.table.retention.models.models.auth;
using com.ataxlab.azure.table.retention.models.models.azuremanagement;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.azure.table.retention.models.models.pagemodel
{
    public class OperatorPageModel
    {
        public OperatorPageModel()
        {

            ApplianceUrl = new List<string>();
            Subscriptions = new List<SubscriptionDTO>();
            Orchestrations = new List<DurableOrchestrationStateDTO>();
            AvailableCommands = new List<AvailableCommand>();
            ApplianceSessionContext = new ApplianceSessionContext();
            IsMustRenderApplianceConfig = true;
        }


        /// <summary>
        /// in debug we need to be able to split brain the html client
        /// to auth to azure, and post to the debug location
        /// </summary>
        public string ApplianceAPIEndPoint { get; set; }
        public string EasyAuthAccessToken { get; set; }

        /// <summary>
        /// populated route template for consumption
        /// by client side ajax
        /// </summary>
        public string QueryWorkflowCheckpointStatusEndpoint { get; set; }
        public string[] AllScopes { get; set; }

        public List<SubscriptionDTO> Subscriptions { get; set; }

        public List<string> ApplianceUrl { get; set; }

        public List<DurableOrchestrationStateDTO> Orchestrations { get; set; }

        public List<AvailableCommand> AvailableCommands { get; set; }
        public string AccessToken { get; set; }

        /// <summary>
        /// signals view logic to show user
        /// subscription and storage account selection
        /// </summary>
        public bool IsMustRenderApplianceConfig { get; set; }
        public ApplianceSessionContext ApplianceSessionContext { get; set; }
        public string ResetDeviceUrl { get; set; }
    }
}
