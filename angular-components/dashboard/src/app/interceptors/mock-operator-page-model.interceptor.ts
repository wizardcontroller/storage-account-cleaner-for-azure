import { Injectable, Injector } from '@angular/core';

import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpResponse
} from '@angular/common/http';
import { Observable, of } from 'rxjs';

const mockWorkflowCheckpoint = {"TimeStamp":"2021-07-02T16:17:03.6679472Z","CurrentCheckpoint":1,"Messsage":"manage your device","AvailableCommands":[{"AvailableCommandId":"a442d06c-1581-4e3a-9c8e-9b647f346376","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":0,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"Please Initialize The Appliance","MenuLabel":"Provision Appliance"},{"AvailableCommandId":"6d00c181-4788-46c4-9d19-266a011c69f1","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":7,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"Delete's The Appliance Configuration","MenuLabel":"Cancel Workflow"},{"AvailableCommandId":"345f9d49-3592-4aaa-abee-5e9bb1f5b3c0","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":4,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"The appliance needs to calculate storage consumption by azure diagnostics over time","MenuLabel":"Inventory Storage Account Entities Usage Over Time"},{"AvailableCommandId":"70a811da-1d74-4341-bb0e-701084ba63db","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":6,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"The appliance needs to enumerate the storage accounts in your available subscriptions","MenuLabel":"Inventory Storage Accounts"},{"AvailableCommandId":"070c4b65-53b0-42e3-869d-e6c288ff50f7","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":3,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"The appliance needs to enumerate the storage accounts in your available subscriptions that have azure diagnostics entities","MenuLabel":"Inventory Storage Account Entities"},{"AvailableCommandId":"691b8768-3bca-4e1f-b75f-eb547ba95ea2","UserOid":"0c0481e0-cb4d-4086-8ef7-3e4930709a27","TenantId":"e14bb7b0-f325-41d8-89c4-c2b5f737c342","SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","subscriptionName":null,"WorkflowOperation":2,"CommandParameterJson":null,"WorklowOperationDisplayMessage":"The appliance needs to enumerate the storage accounts in your available subscriptions","MenuLabel":"Inventory Storage Permissions"}],"SubscriptionId":"c295568c-70fe-4146-8bbc-63ef324fa267","SubscriptionName":null}

const mockOperatorPagemodel = {"applianceAPIEndPoint":null,"easyAuthAccessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdGFibGVfc2lkIjoic2lkOmE3NGQ1MDAyMWFmNGUxM2E1NDhiMGUzNWVkNDcyZmRkIiwic3ViIjoic2lkOjhjMmMyNmQ2Y2FlMWJmOWNjYzQxN2I0MjZhZmM0ZTMwIiwiaWRwIjoiYWFkIiwidmVyIjoiMyIsIm5iZiI6MTYyNDk5MDg0NiwiZXhwIjoxNjI3NTgyODQ2LCJpYXQiOjE2MjQ5OTA4NDYsImlzcyI6Imh0dHBzOi8vd2l6YXJkY29udHJvbGxlci1kZXYtdGFibGUtcmV0ZW50aW9uLWFwcGxpYW5jZS5henVyZXdlYnNpdGVzLm5ldC8iLCJhdWQiOiJodHRwczovL3dpemFyZGNvbnRyb2xsZXItZGV2LXRhYmxlLXJldGVudGlvbi1hcHBsaWFuY2UuYXp1cmV3ZWJzaXRlcy5uZXQvIn0.6kCMwD0Ua7F3C3g2YQBd1arHcBBzshEL7zAdc2_UDBw","queryWorkflowCheckpointStatusEndpoint":"QueryWorkflowCheckpointStatus/0e76962f-fa9d-4b5b-8086-62c8388aac46/576bbe84-5a45-45e3-b578-b97b07d8f9ba","allScopes":null,"subscriptions":[{"isSelected":false,"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe","authorizationSource":"RoleBased","managedByTenants":[],"subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","displayName":"WizardController Pay As You Go","state":"Enabled","subscriptionPolicies":{"locationPlacementId":"Public_2014-09-01","quotaId":"PayAsYouGo_2014-09-01","spendingLimit":"Off"}}],"applianceUrl":["https://wizardcontroller-dev-table-retention-appliance.azurewebsites.net/api/"],"orchestrations":[{"name":"DebugWorkflowEntryPoint","instanceId":"c068459d-d209-ab03-10e3-3ea9953a0264","createdTime":"2021-06-02T03:23:47.3179494Z","lastUpdatedTime":"2021-06-02T03:25:34.8664688Z","input":"{\"$type\":\"com.ataxlab.azure.table.retention.state.entities.ApplianceSessionContextEntity, com.ataxlab.functions.table.retention\",\"Id\":\"44a67fe0-38f7-4425-8bec-139023717ee4\",\"SelectedSubscriptionId\":\"4757ac88-3c82-4e4a-95f1-d317eca697fe\",\"SelectedStorageAccounts\":[{\"$type\":\"com.ataxlab.azure.table.retention.state.entities.StorageAccountEntity, com.ataxlab.functions.table.retention\",\"RequestingAzureAdUserOid\":\"0e76962f-fa9d-4b5b-8086-62c8388aac46\",\"IsSelected\":true,\"StorageAccountKind\":\"StorageV2\",\"SubscriptionId\":null,\"Id\":\"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/saasblobs\",\"Location\":\"eastus\",\"Name\":\"saasblobs\",\"StorageAccountType\":\"Microsoft.Storage/storageAccounts\",\"SkuName\":\"Standard_LRS\",\"TenantId\":\"576bbe84-5a45-45e3-b578-b97b07d8f9ba\"},{\"$type\":\"com.ataxlab.azure.table.retention.state.entities.StorageAccountEntity, com.ataxlab.functions.table.retention\",\"RequestingAzureAdUserOid\":\"0e76962f-fa9d-4b5b-8086-62c8388aac46\",\"IsSelected\":true,\"StorageAccountKind\":\"StorageV2\",\"SubscriptionId\":null,\"Id\":\"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/wizardcontrollerblobs\",\"Location\":\"eastus\",\"Name\":\"wizardcontrollerblobs\",\"StorageAccountType\":\"Microsoft.Storage/storageAccounts\",\"SkuName\":\"Standard_LRS\",\"TenantId\":\"576bbe84-5a45-45e3-b578-b97b07d8f9ba\"}],\"UserOid\":\"0e76962f-fa9d-4b5b-8086-62c8388aac46\",\"TenantId\":\"576bbe84-5a45-45e3-b578-b97b07d8f9ba\",\"OperationResult\":null,\"JobOutput\":[],\"CurrentJobOutput\":{\"$type\":\"com.ataxlab.functions.table.retention.entities.ApplianceJobOutputEntity, com.ataxlab.functions.table.retention\",\"RetentionPolicyJobs\":[]}}","output":"null","inputApplianceContext":{"id":"44a67fe0-38f7-4425-8bec-139023717ee4","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","selectedSubscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","selectedSubscription":null,"selectedStorageAccountId":null,"selectedStorageAccounts":[{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":true,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/saasblobs","location":"eastus","name":"saasblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":null},{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":true,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/wizardcontrollerblobs","location":"eastus","name":"wizardcontrollerblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":null}],"userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","availableSubscriptions":[],"availableStorageAccounts":[],"jobOutput":[],"currentJobOutput":{"id":"00000000-0000-0000-0000-000000000000","tableRetentionResult":null,"tableEntityRetentionResult":null,"tableEntityRetentionPolicy":null,"tableRetentionPolicy":null,"retentionPolicyTuples":[],"retentionPolicyJobs":[]}},"runtimeStatus":0,"customStatus":"\"started\"","history":"null"}],"availableCommands":[{"availableCommandId":"d113fbce-db1f-40ca-9d26-ef4eba83956f","userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","subscriptionName":null,"workflowOperation":0,"commandParameterJson":null,"worklowOperationDisplayMessage":"Please Initialize The Appliance","menuLabel":"Provision Appliance"},{"availableCommandId":"90114d7d-3c31-4b12-94b2-0f20d7a73830","userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","subscriptionName":null,"workflowOperation":7,"commandParameterJson":null,"worklowOperationDisplayMessage":"Delete's The Appliance Configuration","menuLabel":"Cancel Workflow"},{"availableCommandId":"5921fcc7-8019-4d67-abee-3361e0841fce","userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","subscriptionName":null,"workflowOperation":6,"commandParameterJson":null,"worklowOperationDisplayMessage":"The appliance needs to enumerate the storage accounts in your available subscriptions","menuLabel":"Inventory Storage Accounts"},{"availableCommandId":"06b10b8e-1ce4-4dc5-8f41-0d2e546a9902","userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","subscriptionName":null,"workflowOperation":2,"commandParameterJson":null,"worklowOperationDisplayMessage":"The appliance needs to enumerate the storage accounts in your available subscriptions","menuLabel":"Inventory Storage Permissions"}],"impersonationToken":"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuYXp1cmUuY29tIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNTc2YmJlODQtNWE0NS00NWUzLWI1NzgtYjk3YjA3ZDhmOWJhLyIsImlhdCI6MTYyNDk5MDU0NiwibmJmIjoxNjI0OTkwNTQ2LCJleHAiOjE2MjQ5OTQ0NDYsImFjciI6IjEiLCJhaW8iOiJBVFFBeS84VEFBQUEwVWZPcmd5SnhTbUZUQ0RqTHVBZFI2MnhLSWZLNGUvWm5TREtWd3pPamdBVm9aRDZ2ZCtuZGJKVlZKZVEvODA0IiwiYW1yIjpbInB3ZCJdLCJhcHBpZCI6IjVkODUyZjI2LTgwMmItNDA1Yy05MzEwLWI4MGQzMDQyNTVjMSIsImFwcGlkYWNyIjoiMSIsImdyb3VwcyI6WyJmOGQxM2RlNS1kMmU1LTQ4YzktYTU0MS0zZTdlNzMwZmJmMDAiLCJhY2Q4MWY2Ni0zOTIwLTRkMjktYmU4Zi00Y2Y2NzBmZDQzNDMiLCJlNWJjOGI5My0yYWM3LTRhNzYtOTdkYS1mN2UxMTVmMThhZGYiLCJiMjdmZDQ3Ny1hNTQ4LTQ4NzAtYjQ0YS0wZWYyOTczMGI1MDQiLCJlOTdlODdkNy1kMTEyLTRlMmQtOWRjMC00MzMyZDFiNTM2ZTAiLCJkMjJjYWI4Yi0wNmM1LTQ0MGEtYjA0NS05OGU2NTJmOWMxZDEiLCI5YjgxNDY3ZS04YTNhLTQ2NGEtYjFjYy05ZGMxN2U1NDQ5Y2QiLCJhZTI0NTU2OC0yNzQ0LTRhYzAtYmRiZC1hNjljYzYwNGZmNWMiXSwiaXBhZGRyIjoiOTkuMjI3LjcwLjE5OSIsIm5hbWUiOiJEZXZvcHMgR2xvYmFsIEFkbWluIiwib2lkIjoiMGU3Njk2MmYtZmE5ZC00YjViLTgwODYtNjJjODM4OGFhYzQ2IiwicHVpZCI6IjEwMDMyMDAwN0EyQkJFRDYiLCJyaCI6IjAuQVRnQWhMNXJWMFZhNDBXMWVMbDdCOWo1dWlZdmhWMHJnRnhBa3hDNERUQkNWY0U0QUVrLiIsInNjcCI6InVzZXJfaW1wZXJzb25hdGlvbiIsInN1YiI6IkVuN0VFRVpkSkpZWGZKeEY3eEZ4MFk2OVN0MHpkQnFVVG9wSVcwWm9NVzQiLCJ0aWQiOiI1NzZiYmU4NC01YTQ1LTQ1ZTMtYjU3OC1iOTdiMDdkOGY5YmEiLCJ1bmlxdWVfbmFtZSI6ImRldm9wc3dpemFyZEB3aXphcmRjb250cm9sbGVyLmNvbSIsInVwbiI6ImRldm9wc3dpemFyZEB3aXphcmRjb250cm9sbGVyLmNvbSIsInV0aSI6InVDeHczUWZxTzBlaEJIdWxZLUJHQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbIjYyZTkwMzk0LTY5ZjUtNDIzNy05MTkwLTAxMjE3NzE0NWUxMCIsIjdiZTQ0YzhhLWFkYWYtNGUyYS04NGQ2LWFiMjY0OWUwOGExMyIsIjlmMDYyMDRkLTczYzEtNGQ0Yy04ODBhLTZlZGI5MDYwNmZkOCIsImIwZjU0NjYxLTJkNzQtNGM1MC1hZmEzLTFlYzgwM2YxMmVmZSIsImU4NjExYWI4LWMxODktNDZlOC05NGUxLTYwMjEzYWIxZjgxNCIsImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfdGNkdCI6MTU0ODAyMTY3NX0.BAuaRynkSjf9YZgBF58puEXYowUcS5oBR02d-k4Zxa-rj7Bx0hP-iZXrXuG9sc2bKlGVhhkHiErZV52xMzzSmEid92xEA9K970Zn0ZzMojw0lB2-fEokQwX8agPdJVqPsGNChLQJSilXoTHzTrPhuzES4Kew0N-LkcVLGpx5Sln7uiCtvoAw1WFz7x7RR-TCSThjVbe31iGp26gPymI9xt3ODpxXW4AWv2hP38n3GfnZdt_uXhv-XtJ5wnKH32V6M9JCMQW0qeuiexxzcpWI-ACZePmFKs0Cm2srRR4XnvMfqnPx4hkvI8gVV3hYPO8Iw6HixncRrViAbagCrsBsAQ","selectedSubscriptionId":null,"isMustRenderApplianceConfig":false,"applianceSessionContext":{"id":"44a67fe0-38f7-4425-8bec-139023717ee4","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","selectedSubscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","selectedSubscription":null,"selectedStorageAccountId":null,"selectedStorageAccounts":[{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":true,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/saasblobs","location":"eastus","name":"saasblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe"}],"userOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","availableSubscriptions":[{"isSelected":false,"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe","authorizationSource":"RoleBased","managedByTenants":[],"subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","displayName":"WizardController Pay As You Go","state":"Enabled","subscriptionPolicies":{"locationPlacementId":"Public_2014-09-01","quotaId":"PayAsYouGo_2014-09-01","spendingLimit":"Off"}}],"availableStorageAccounts":[{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":false,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/saasblobs","location":"eastus","name":"saasblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe"},{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":false,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/wizardcontrollerblobs","location":"eastus","name":"wizardcontrollerblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe"}],"jobOutput":[],"currentJobOutput":{"id":"00000000-0000-0000-0000-000000000000","tableRetentionResult":null,"tableEntityRetentionResult":null,"tableEntityRetentionPolicy":null,"tableRetentionPolicy":null,"retentionPolicyTuples":[],"retentionPolicyJobs":[{"id":"94afae8c-fecc-4a01-88f0-6e72f85830f2","tableStorageRetentionPolicy":{"id":"df42845b-10fb-4883-9c68-9bbafda968db","tableStorageEntityRetentionPolicy":{"id":"01019f80-d53b-4e02-aa97-116ad59e0e2d","diagnosticsRetentionSurface":{"id":"26758748-3f2c-431d-b437-34ecb2056ede","diagnosticsRetentionSurfaceEntities":[{"itemDescriptor":0,"itemType":0,"itemDescription":"Syslog, Linux Virtual Machines","itemExists":false,"itemWillBeDeleted":false,"id":"e1c40dd0-ce94-4800-b55f-f853f5de405a","storageAccountId":null,"suscriptionId":null,"tableName":"LinuxsyslogVer2v0","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":6,"itemType":0,"itemDescription":"Windows Event Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles","itemExists":false,"itemWillBeDeleted":false,"id":"7a8fb97e-375c-4897-a298-81f858642a42","storageAccountId":null,"suscriptionId":null,"tableName":"WADWindowsEventLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":7,"itemType":0,"itemDescription":"Windows ETW Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles","itemExists":false,"itemWillBeDeleted":false,"id":"a154cbac-7bf3-4431-a423-0666888071c6","storageAccountId":null,"suscriptionId":null,"tableName":"WADETWEventTable","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":4,"itemType":0,"itemDescription":"Logs written in code using the trace listener.","itemExists":false,"itemWillBeDeleted":false,"id":"70fab46d-c51d-4f1b-980b-3643831a3ecd","storageAccountId":null,"suscriptionId":null,"tableName":"WadLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":8,"itemType":0,"itemDescription":"Diagnostic monitor and configuration changes.","itemExists":false,"itemWillBeDeleted":false,"id":"b666627e-eefa-4af5-8c0e-e781b4bf1180","storageAccountId":null,"suscriptionId":null,"tableName":"WADDiagnosticInfrastructureLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":8,"itemType":0,"itemDescription":"Diagnostic monitor and configuration changes.","itemExists":false,"itemWillBeDeleted":false,"id":"d259778c-4eaa-4484-b568-acb52d50f1f4","storageAccountId":null,"suscriptionId":null,"tableName":"WADDiagnosticInfrastructureLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":5,"itemType":0,"itemDescription":"Performance counters.","itemExists":false,"itemWillBeDeleted":false,"id":"aef0a7bf-aa3e-461d-896e-af0314e1edf3","storageAccountId":null,"suscriptionId":null,"tableName":"WADPerformanceCountersTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"}]},"policyEnforcementMode":0,"numberOfDays":30,"oldestRetainedEntity":"0001-01-01T00:00:00","mostRecentRetainedEntity":"0001-01-01T00:00:00","tableNames":["LinuxsyslogVer2v0","WADWindowsEventLogsTable","WADETWEventTable","WadLogsTable","WADDiagnosticInfrastructureLogsTable","WADDiagnosticInfrastructureLogsTable","WADPerformanceCountersTable"]},"tableStorageTableRetentionPolicy":{"id":"9973267d-0e8c-4960-b94a-bc3f35c025bd","metricRetentionSurface":{"id":"5ca45779-3ac0-41e3-a36d-05308cc49418","aggregationPrefixes":["PT1H","PT1M"],"metricsRetentionSurfaceItemEntities":[]},"policyEnforcementMode":0,"deleteOlderTablesThanCurrentMonthMinusThis":2,"oldestRetainedTable":"0001-01-01T00:00:00","mostRecentRetainedTable":"0001-01-01T00:00:00","tableNames":[]}},"storageAccount":{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":true,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/saasblobs","location":"eastus","name":"saasblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":"576bbe84-5a45-45e3-b578-b97b07d8f9ba","subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe"},"tableStoragePolicyEnforcementResult":{"id":"00000000-0000-0000-0000-000000000000","policy":{"id":"3bb9dabf-9245-42a0-b2be-2070d21052a2","metricRetentionSurface":{"id":"b89ec356-0b4b-4958-b447-2f7fd02a99a9","aggregationPrefixes":["PT1H","PT1M"],"metricsRetentionSurfaceItemEntities":[]},"policyEnforcementMode":0,"deleteOlderTablesThanCurrentMonthMinusThis":12,"oldestRetainedTable":"0001-01-01T00:00:00","mostRecentRetainedTable":"0001-01-01T00:00:00","tableNames":[]},"policyTriggerCount":0},"tableStorageEntityPolicyEnforcementResult":{"id":"00000000-0000-0000-0000-000000000000","policy":{"id":"04fb279b-efc3-481f-afad-0f2aa8700b56","diagnosticsRetentionSurface":{"id":"ae6e8ceb-78f6-4eb9-ba51-23b65c464db8","diagnosticsRetentionSurfaceEntities":[]},"policyEnforcementMode":0,"numberOfDays":30,"oldestRetainedEntity":"0001-01-01T00:00:00","mostRecentRetainedEntity":"0001-01-01T00:00:00","tableNames":[]},"policyTriggerCount":0}},{"id":"27bb31c0-f4b3-40a7-92f2-23cdcb570691","tableStorageRetentionPolicy":{"id":"471b018d-7004-4cce-b43b-4ce15a7197a4","tableStorageEntityRetentionPolicy":{"id":"896dc0c5-99a9-4b40-b97c-3ad3e9acab67","diagnosticsRetentionSurface":{"id":"929a5df9-3d08-4a49-8eda-f0d108b34f2e","diagnosticsRetentionSurfaceEntities":[{"itemDescriptor":0,"itemType":0,"itemDescription":"Syslog, Linux Virtual Machines","itemExists":false,"itemWillBeDeleted":false,"id":"b042ca95-478a-4e75-a5de-58c48f95e586","storageAccountId":null,"suscriptionId":null,"tableName":"LinuxsyslogVer2v0","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":6,"itemType":0,"itemDescription":"Windows Event Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles","itemExists":false,"itemWillBeDeleted":false,"id":"d080a8ba-801f-4f40-9840-efbc2f807492","storageAccountId":null,"suscriptionId":null,"tableName":"WADWindowsEventLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":7,"itemType":0,"itemDescription":"Windows ETW Logs - Service Fabric, Virtual Machines, Web Roles, Worker Roles","itemExists":false,"itemWillBeDeleted":false,"id":"0939b535-c99a-4cec-a35a-780b1680ce3a","storageAccountId":null,"suscriptionId":null,"tableName":"WADETWEventTable","documentationLink":"https://docs.microsoft.com/en-us/azure/azure-monitor/agents/diagnostics-extension-logs"},{"itemDescriptor":4,"itemType":0,"itemDescription":"Logs written in code using the trace listener.","itemExists":false,"itemWillBeDeleted":false,"id":"8401c2e8-bdb1-45a3-8bde-974fc1ef8262","storageAccountId":null,"suscriptionId":null,"tableName":"WadLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":8,"itemType":0,"itemDescription":"Diagnostic monitor and configuration changes.","itemExists":false,"itemWillBeDeleted":false,"id":"a04604cd-5907-4389-9033-14f74bfc0ad2","storageAccountId":null,"suscriptionId":null,"tableName":"WADDiagnosticInfrastructureLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":8,"itemType":0,"itemDescription":"Diagnostic monitor and configuration changes.","itemExists":false,"itemWillBeDeleted":false,"id":"f300808e-54ae-4587-9695-2ea8b8d135f7","storageAccountId":null,"suscriptionId":null,"tableName":"WADDiagnosticInfrastructureLogsTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"},{"itemDescriptor":5,"itemType":0,"itemDescription":"Performance counters.","itemExists":false,"itemWillBeDeleted":false,"id":"e947eebe-a8d9-4fef-b716-ade7a4f521c2","storageAccountId":null,"suscriptionId":null,"tableName":"WADPerformanceCountersTable","documentationLink":"https://docs.microsoft.com/en-us/azure/cloud-services/diagnostics-extension-to-storage#tools-to-view-diagnostic-data"}]},"policyEnforcementMode":0,"numberOfDays":30,"oldestRetainedEntity":"0001-01-01T00:00:00","mostRecentRetainedEntity":"0001-01-01T00:00:00","tableNames":["LinuxsyslogVer2v0","WADWindowsEventLogsTable","WADETWEventTable","WadLogsTable","WADDiagnosticInfrastructureLogsTable","WADDiagnosticInfrastructureLogsTable","WADPerformanceCountersTable"]},"tableStorageTableRetentionPolicy":{"id":"77df3ea1-5c90-4fba-8449-caa2c6a0b4df","metricRetentionSurface":{"id":"2ce6d331-2cc4-41cd-88bc-ad33a98c4401","aggregationPrefixes":["PT1H","PT1M"],"metricsRetentionSurfaceItemEntities":[]},"policyEnforcementMode":0,"deleteOlderTablesThanCurrentMonthMinusThis":2,"oldestRetainedTable":"0001-01-01T00:00:00","mostRecentRetainedTable":"0001-01-01T00:00:00","tableNames":[]}},"storageAccount":{"requestingAzureAdUserOid":"0e76962f-fa9d-4b5b-8086-62c8388aac46","isSelected":false,"storageAccountKind":"StorageV2","id":"/subscriptions/4757ac88-3c82-4e4a-95f1-d317eca697fe/resourceGroups/saas_rg/providers/Microsoft.Storage/storageAccounts/wizardcontrollerblobs","location":"eastus","name":"wizardcontrollerblobs","storageAccountType":"Microsoft.Storage/storageAccounts","skuName":"Standard_LRS","tenantId":null,"subscriptionId":"4757ac88-3c82-4e4a-95f1-d317eca697fe"},"tableStoragePolicyEnforcementResult":{"id":"00000000-0000-0000-0000-000000000000","policy":{"id":"62e1052c-d75e-4cac-8eb5-13af947f1610","metricRetentionSurface":{"id":"ecfa227a-6ea4-4b9d-843e-3ae929db8a33","aggregationPrefixes":["PT1H","PT1M"],"metricsRetentionSurfaceItemEntities":[]},"policyEnforcementMode":0,"deleteOlderTablesThanCurrentMonthMinusThis":12,"oldestRetainedTable":"0001-01-01T00:00:00","mostRecentRetainedTable":"0001-01-01T00:00:00","tableNames":[]},"policyTriggerCount":0},"tableStorageEntityPolicyEnforcementResult":{"id":"00000000-0000-0000-0000-000000000000","policy":{"id":"2f761af6-7f36-4171-b91f-b425b0bad3cc","diagnosticsRetentionSurface":{"id":"f3a0172e-bc5e-4bc7-9cce-823d65d5b4b6","diagnosticsRetentionSurfaceEntities":[]},"policyEnforcementMode":0,"numberOfDays":30,"oldestRetainedEntity":"0001-01-01T00:00:00","mostRecentRetainedEntity":"0001-01-01T00:00:00","tableNames":[]},"policyTriggerCount":0}}]}},"resetDeviceUrl":"DeleteWorkflowCheckpointEditMode/0e76962f-fa9d-4b5b-8086-62c8388aac46/576bbe84-5a45-45e3-b578-b97b07d8f9ba"}

/**
 * supports local dev scenarios when the window origin is
 * one of the well known angular dev ports
 */
@Injectable()
export class MockOperatorPageModelInterceptor implements HttpInterceptor {

  constructor(private injector: Injector) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<any>> {
    const requestClone = request.clone();
    if(
      window.origin.includes(":4200")
      ||
      window.origin.includes(":9876")      )
      {
        // we will mock requests in dev environments
        // mock operatorPageModel
        if(requestClone.url.toLowerCase().includes("getoperatorpage"))
        {
          console.log("returning mocked pagemodel");
          return of(new HttpResponse({status: 200, body: mockOperatorPagemodel}));
        }
      }
      else{
        console.log("mock interceptor not running");
      }

    return next.handle(request);
  }
}
