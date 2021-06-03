using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.ataxlab.azure.table.retention.models
{
    //
    // Summary:
    //     The URIs that are used to perform a retrieval of a public blob, queue, table,
    //     web or dfs object.
    public class EndpointsDto
    {
        //
        // Summary:
        //     Initializes a new instance of the Endpoints class.
        public EndpointsDto() { }
        //
        // Summary:
        //     Initializes a new instance of the Endpoints class.
        //
        // Parameters:
        //   blob:
        //     Gets the blob endpoint.
        //
        //   queue:
        //     Gets the queue endpoint.
        //
        //   table:
        //     Gets the table endpoint.
        //
        //   file:
        //     Gets the file endpoint.
        //
        //   web:
        //     Gets the web endpoint.
        //
        //   dfs:
        //     Gets the dfs endpoint.
        public EndpointsDto(string blob = null, string queue = null, string table = null, string file = null, string web = null, string dfs = null)
        {
            this.Blob = blob;
            this.Queue = queue;
            this.Table = table;
            this.File = file;
            this.Web = web;
            this.Dfs = dfs;
        }

        //
        // Summary:
        //     Gets the blob endpoint.
        [JsonProperty(PropertyName = "blob")]
        public string Blob { get; set; }
        //
        // Summary:
        //     Gets the queue endpoint.
        [JsonProperty(PropertyName = "queue")]
        public string Queue { get; set; }
        //
        // Summary:
        //     Gets the table endpoint.
        [JsonProperty(PropertyName = "table")]
        public string Table { get; set; }
        //
        // Summary:
        //     Gets the file endpoint.
        [JsonProperty(PropertyName = "file")]
        public string File { get; set; }
        //
        // Summary:
        //     Gets the web endpoint.
        [JsonProperty(PropertyName = "web")]
        public string Web { get; set; }
        //
        // Summary:
        //     Gets the dfs endpoint.
        [JsonProperty(PropertyName = "dfs")]
        public string Dfs { get; set; }
    }

    public class PublicEndpointsDto
    {
        public PublicEndpointsDto() {

            //this.Primary = new EndpointsDto();
            //this.Secondary = new EndpointsDto();
        }

        public EndpointsDto Primary { get; set; }
        public EndpointsDto Secondary { get; set; }
    }

    /// <summary>
    /// decouple our model form the sdks involved
    /// </summary>
    public class StorageAccountModel
    {
        public string Name { get; set;
}
        public Kind Kind { get; set; }

        public PublicEndpointsDto EndPoints { get; set; }

        public IReadOnlyList<StorageAccountKey> Keys { get; set; }
    }
}
