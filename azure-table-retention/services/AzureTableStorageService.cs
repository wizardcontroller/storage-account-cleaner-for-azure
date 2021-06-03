using com.ataxlab.azure.table.retention.models.control;
using com.ataxlab.functions.table.retention.entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace com.ataxlab.functions.table.retention.services
{  
    public class GenericTableEntity : TableEntity
    {
        /// <summary>
        /// generic object to represent any row in table storage
        /// </summary>
        public GenericTableEntity() : base()
        {

        }
    }

    public interface IAzureTableStorageService
    {
    }

    public class AzureTableStorageService : IAzureTableStorageService
    {
        public HttpClient CurrentHttpClient { get; set; }
        
        public AzureTableStorageService(HttpClient httpClient)
        {
            CurrentHttpClient = httpClient;
        }

        public async Task<string> QueryTableStorage(string token, Uri storageUrl, string query)
        {
            string ret = string.Empty;
   

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
   

                this.CurrentHttpClient.DefaultRequestHeaders.Clear();

                this.CurrentHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // .Add("Authorization", "Bearer " + token);
                this.CurrentHttpClient.DefaultRequestHeaders.Add("Host", "management.azure.com");
                this.CurrentHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var request = new HttpRequestMessage(HttpMethod.Get, "/subscriptions?api-version=2020-01-01");
                System.Diagnostics.Trace.WriteLine("request message configured for management rest {0}", this.CurrentHttpClient.BaseAddress.AbsoluteUri.ToString());

                var response = await this.CurrentHttpClient.GetStringAsync(request.RequestUri);
                ret = response;
            }

            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

            return ret;
        }

        public CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        public async Task GetMetricsTableResult(MetricsRetentionSurfaceItemEntity item)
        {
            var tableName = item.TableName;
            string storageConnectionString = ""; //AppSettings.LoadAppSettings().StorageConnectionString;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity> query = 
                new TableQuery<Microsoft.WindowsAzure.Storage.Table.TableEntity>() { TakeCount = 1000 }
                .Where(TableQuery.GenerateFilterCondition("Timestamp", QueryComparisons.LessThanOrEqual, item.PolicyAgeTriggerInMonths.ToString()))
                .Select(new List<string>() { "PartitionKey", "RowKey" });

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
       
        }
    }

}
