using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.ataxlab.functions.table.retention.dashboard.options
{
    public class ClientCertificate
    {
        public string SourceType { get; set; }
        public string KeyVaultUrl { get; set; }
        public string KeyVaultCertificateName { get; set; }
    }

    public class AzureAd
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public List<ClientCertificate> ClientCertificates { get; set; }
        public string ClientSecret { get; internal set; }
    }
}
