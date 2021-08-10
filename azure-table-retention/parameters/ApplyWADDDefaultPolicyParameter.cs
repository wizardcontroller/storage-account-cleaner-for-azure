
using com.ataxlab.azure.table.retention.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.functions.table.retention.parameters
{
    /// <summary>
    /// start the discovery and application
    /// of the default policy retention policy
    /// that is targeted at Windows Azure Diagnostic
    /// Logs
    /// </summary>
    public class ApplyWADDDefaultPolicyParameter
    {
        public ApplyWADDDefaultPolicyParameter()
        {
            Policies = new List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>>();
            EntityRetentionAgeInDays = 999;
            PolicyEnforcementMode = policyEnforcementMode.WhatIf;
        }

        /// <summary>
        /// defaults to WHATIF, nothing should be deleted
        /// </summary>
        public policyEnforcementMode PolicyEnforcementMode { get; set; }

        /// <summary>
        /// absurd default value to give you a chance
        /// </summary>
        public int EntityRetentionAgeInDays { get; set; }

        public List<Tuple<TableStorageRetentionPolicy, StorageAccountModel>> Policies { get; set; }
    }
}
