    using com.ataxlab.azure.table.retention.models.automapper;
using com.ataxlab.functions.table.retention.services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using com.ataxlab.azure.table.retention.services.authorization;
using Microsoft.Azure.WebJobs;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using com.ataxlab.azure.table.retention.models.control;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

[assembly: FunctionsStartup(typeof(com.ataxlab.functions.table.retention.Startup))]
namespace com.ataxlab.functions.table.retention
{
    //class ConfigNameResolver : INameResolver
    //{
    //    public string Resolve(string name)
    //    {
    //        return ConfigurationManager.AppSettings[name];
    //    }
    //}
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            try
            {
                BuildConfig(builder);

                // disable performance counters
                // as per https://stackoverflow.com/questions/42890719/how-to-disable-standard-performance-counters-in-application-insights

                // maybe this breaks http routes
                //var serviceDescriptor = builder.Services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(PerformanceCollectorModule));
                //builder.Services.Remove(serviceDescriptor);
            }
            catch (Exception e)
            {
                Console.WriteLine("startup failure exception : " + e.Message);
            }
        }

        private static void BuildConfig(IFunctionsHostBuilder builder)
        {
            // as per https://github.com/Azure/azure-webjobs-sdk/issues/2063
            //var configNameResolver = new ConfigNameResolver();
            //builder.Services.AddSingleton(configNameResolver);

            var configuration = builder.GetContext().Configuration;
            //builder.Services.AddSingleton<IConfiguration>(configuration);

            //    AzureADJwtBearerValidation jwtValidator = new AzureADJwtBearerValidation()
            //    {
            //        _tenantId = configuration["AzureAd:TenantId"],
            //        _audience = configuration["AzureAd:ClientId"],
            //        _instance = configuration["AzureAd:Instance"]
            //};


            //builder.Services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
            //           ForwardedHeaders.XForwardedHost | // as per https://soapfault.com/2020/02/24/asp-net-core-reverse-proxy-and-x-forwarded-headers/
            //        ForwardedHeaders.XForwardedProto;
            //    // Only loopback proxies are allowed by default.
            //    // Clear that restriction because forwarders are enabled by explicit 
            //    // configuration.
            //    options.ForwardLimit = 2; // as per https://soapfault.com/2020/02/24/asp-net-core-reverse-proxy-and-x-forwarded-headers/
            //    options.KnownNetworks.Clear();
            //    options.KnownProxies.Clear();
            //});
            builder.Services.AddScoped<AzureADJwtBearerValidation>();

            // task hub names are environment dependent
            // var devEnvironment = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")?.ToLower()?.Contains("devel");

            // survive a missing task hub setting in app settings
            //var hubnameSetting = configuration[ControlChannelConstants.TASKHUBNAME] == null || configuration[ControlChannelConstants.TASKHUBNAME] == string.Empty
            //                     ? "tableretentionhub" : configuration[ControlChannelConstants.TASKHUBNAME];
            //var hub = devEnvironment == true ? "TestHubName" : hubnameSetting;

            // inject the hub configureation for injectable classes
            // that use durableentityclientfactory, which needs a hub name
            // materialized PRIOR to constructor injection
            //builder.Services.AddSingleton<DurableTaskOptions>(opts =>
            //{
            //    return new DurableTaskOptions()
            //    {
            //        HubName = hub
            //    };
            //});

            //builder.Services.AddDurableClientFactory(opts =>
            //{
            //    opts.ConnectionName = "AzureWebJobsStorage";
            //    opts.TaskHub = hub;
            //    opts.IsExternalClient = false;
            //});

            // maybe this breaks http routes and causs 404
            // builder.Services.AddSingleton<IMessageSerializerSettingsFactory, CustomMessageSerializerSettingsFactory>();

            // one per appdomain
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            string clientSecret = String.Empty;
            string clientId = String.Empty;
            string tenantId = String.Empty;
            string telemetryKey = String.Empty;
            string subscriptionId = String.Empty;

            clientSecret = configuration["ServicePrincipalPassword"];
            clientId = configuration["ServicePrincipalId"];
            tenantId = configuration["TenantId"];
            telemetryKey = configuration["ApplicationInsightsKey"];
            subscriptionId = configuration["SubscriptionKey"];

            Console.WriteLine("client secret {0}", clientSecret);
            Console.WriteLine("client clientId {0}", clientId);
            Console.WriteLine("client tenantId {0}", tenantId);
            Console.WriteLine("client telemetryKey {0}", telemetryKey);
            Console.WriteLine("client secret {0}", subscriptionId);

            if (builder.GetContext().EnvironmentName.Equals("Development"))
            {

                var telemetryConfiguraition = new TelemetryConfiguration(telemetryKey);
                builder.Services.AddSingleton<TelemetryConfiguration>(telemetryConfiguraition);
            }
   
         

            // DON'T do this as per https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
            // var telemmetryClient = new TelemetryClient(new TelemetryConfiguration(telemetryKey));
            // builder.Services.AddSingleton<TelemetryClient>(telemmetryClient);
            // builder.Services.AddApplicationInsightsTelemetry();

            AzureCredentials cred = null;
            try
            {
                cred = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud)
                            .WithDefaultSubscription(subscriptionId);


                builder.Services.AddSingleton<Microsoft.Azure.Management.ResourceManager.Fluent.Authentication.AzureCredentials>(cred);
            }
            catch (Exception e)
            {
                // Console.WriteLine("exception setting up azure credentials")
            }



            builder.Services.AddLogging();

  
            builder.Services.AddScoped<ITableEntityRetentionClient, TableEntityRetentionClient>();

            builder.Services.AddScoped<ITableRetentionApplianceActivities, TableRetentionApplianceActivities>();


            builder.Services.AddScoped<ITableRetentionApplianceEngine, TableRetentionApplianceEngine>();
         }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
          
           
            builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"local.settings.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();


        }


        /// <summary>
        /// A factory that provides the serialization for all inputs and outputs for activities and
        /// orchestrations, as well as entity state.
        /// </summary>
        internal class CustomMessageSerializerSettingsFactory : IMessageSerializerSettingsFactory
        {
            public JsonSerializerSettings CreateJsonSerializerSettings()
            {
                // Return your custom JsonSerializerSettings here
                return new JsonSerializerSettings()
                {
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor

                };
            }
        }

    }
}
