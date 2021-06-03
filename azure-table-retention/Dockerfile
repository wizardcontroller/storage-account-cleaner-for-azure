#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


FROM mcr.microsoft.com/azure-functions/dotnet:3.0-appservice AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# FROM mcr.microsoft.com/dotnet/core/sdk:3.1.407-buster AS build
FROM mcr.microsoft.com/dotnet/sdk:3.1-buster AS build

WORKDIR /src
COPY ["azure-table-retention/azure.table.retention.appliance.csproj", "azure-table-retention/"]

RUN dotnet restore "azure-table-retention/azure.table.retention.appliance.csproj"  -r "linux-x64" 
COPY . .
WORKDIR "/src/azure-table-retention"
RUN dotnet build "azure.table.retention.appliance.csproj"  -r "linux-x64" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "azure.table.retention.appliance.csproj" -r "linux-x64" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot

COPY --from=publish /app/publish .


ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true     \
    ASPNETCORE_URLS=http://*:80