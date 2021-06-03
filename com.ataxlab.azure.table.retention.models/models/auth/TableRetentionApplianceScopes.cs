using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models.models.auth
{
    /// <summary>
    /// these are some pretty severe scopes being requested
    /// so when you concent you'll think about what this application is doing
    /// as per https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/3-WebApp-multi-APIs
    /// </summary>
    public class TableRetentionApplianceScopes
    {
        public TableRetentionApplianceScopes() { }

        private string _ApplianceAppUri = string.Empty;
        // you MUST inject the ApplianceAppUri
        public string ApplianceAppUri {
                                        get
                                        {
                                            return _ApplianceAppUri;
                                        }
                                        set
                                        {
                                            _ApplianceAppUri = value == null ? String.Empty : value.TrimEnd('/');
                                        }
                                      }

        public string[] DefaultApplianceScope { get { return new string[] { String.Format("{0}/.default", ApplianceAppUri), String.Format("{0}/offline_access", ApplianceAppUri) }; } }
        public string[] ApplianceUserImpersonationScope { get { return new string[] { String.Format("{0}/user_impersonation", ApplianceAppUri) }; } }

        public string StorageAccountRead { get { return String.Format("{0}/Storage.Account.Read", ApplianceAppUri); } } 
        public  string StorageAccountList { get { return String.Format("{0}/Storage.Account.List", ApplianceAppUri); } } 
        public  string StorageTableList { get { return String.Format("{0}/Storage.Table.List", ApplianceAppUri); } } 
        public  string StorageTableRead { get { return String.Format("{0}/Storage.Table.Read", ApplianceAppUri); } } 
        public  string StorageTableEntityDelete { get { return String.Format("{0}/Storage.Table.Entity.Delete", ApplianceAppUri); } } 
        public  string StorageTableDelete { get { return String.Format("{0}/Storage.Table.Delete", ApplianceAppUri); } }

        public string ManageAppliance { get { return String.Format("{0}/Manage.Appliance", ApplianceAppUri); } }

        private string[] AllScopes
        {
            get
            {
                return new string[]  
                {
                            StorageAccountRead,
                            StorageAccountList,
                            StorageTableList,
                            StorageTableRead,
                            StorageTableEntityDelete,
                            StorageTableDelete
                          };
                }
            }
        }
}
