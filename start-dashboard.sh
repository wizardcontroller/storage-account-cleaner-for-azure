docker run -it -p 80:80 -p 443:443  wizardcontrolleracr.azurecr.io/azure-table-retention-dashboard:dev \
-e "AzureAd:RedirectUrl=http://localhost" \
-e "WEBSITE__CORS__ALLOWED__ORIGINS=https://functions.azure.com,https://functions-staging.azure.com,https://functions-next.azure.com,https://localhost:44349" \
-e "WEBSITE_CORS_SUPPORT_CREDENTIALS=True" -e "AzureAd:TenantId=" \
-e "AzureAd:Instance=https://login.microsoftonline.com/" -e "AzureAd:Domain="  \
-e "AzureAd:ClientSecret=" -e "AzureAd:ClientId=" \
-e "ApplianceBaseUrl=" -e "ApplianceAppUri=" 
