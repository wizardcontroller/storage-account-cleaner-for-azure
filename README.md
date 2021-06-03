# Introduction 
this is a tool for applying retention policies on Azure Storage Tables

the need arises because of things like this https://feedback.azure.com/forums/231545-diagnostics-and-monitoring/suggestions/37520419-add-way-to-clean-old-logs

so i adapted some code from here https://mysharepointlearnings.wordpress.com/2019/08/20/managing-azure-vm-diagnostics-data-in-table-storage/

and i had to delve into the current state of the branch of this https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/identity/Azure.Identity#credential-classes

and i found out that the fluent azure sdk api is cool like they say here https://build5nines.com/fluent-api-libraries-for-azure-net-sdk/

i wanted to make the tool run without secrets in config files so i had to look here https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md

but a tool that automagically starts deleting tables and entities from your storage account based only on the roles you assign the service principal of the function app it's running in may not be so cool unless you really really really trust that code.

mind you, it's not magic. 

there's TableStorageTableRetentionPolicy that is there to age tables by deleting them

and there's TableStorageEntityRetentionPolicy that is there to age entities in tables by deleting old entities

and there had to be this where a static list of tables used by Azure Monitor had ot be defined.

    public class DefaultTableRetentionConfiguration : TableRetentionConfiguration
    {
        public static String WADMetrics = "WADMetrics";
        public static String WADDiagnosticInfrastructureLogsTable = "WADDiagnosticInfrastructureLogsTable";
        public static String WADWindowsEventLogsTable = "WADWindowsEventLogsTable";
        public static String WADPerformanceCountersTable = "WADPerformanceCountersTable";

        // plus various other interesting bits of code 
    }

so the automagic is ultimately driven by string comparisons against the names of storage accounts enumerated this (TableEntityRetentionClient) fancy class which uses a service principal whose details you must supply and configure with Storage Read permissions

anyhow in closing for now if you ever need it, use dependency injection with your azure functions man https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection


# Getting Started
Right now don't use this

# Build and Test
if you're all that clone the code and make sure you have your local development toolchain for azure functions all together

you'll need to cope with this requirement in your settings

            var clientSecret = LocalSettings["values:ServicePrincipalPassword"];
            var clientId = LocalSettings["values:ServicePrincipalId"];
            var tenantId = LocalSettings["values:TenantId"];
            var telemetryKey = LocalSettings["values:ApplicationInsightsKey"];
            var subscriptionId = LocalSettings["values:SubscriptionKey"];

# Contribute
Feel free to make suggestions and code contributions